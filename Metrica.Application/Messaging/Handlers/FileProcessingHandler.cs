using Metrica.Application.Excel;
using Metrica.Application.Interfaces;
using Metrica.Application.Interfaces.Excel;
using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Application.Interfaces.Storage;
using Metrica.Application.Messaging.Contracts;
using Metrica.Domain.Entities;
using Metrica.Domain.Enums;
using System.Globalization;

namespace Metrica.Application.Messaging.Handlers
{
    public sealed class FileProcessingHandler : IFileProcessingHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IFileLoadStatusHistoryRepository _statusHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorage _fileStorage;
        private readonly IProductExcelReader _productExcelReader;
        private readonly IFileLoadErrorRepository _fileLoadErrorRepository;
        private readonly IProcessedProductRepository _processedProductRepository;
        private readonly IFileLoadNotificationOutbox _notificationOutbox;


        public FileProcessingHandler(
            IFileLoadRepository fileLoadRepository,
            IFileLoadStatusHistoryRepository statusHistoryRepository,
            IUnitOfWork unitOfWork,
            IFileStorage fileStorage,
            IProductExcelReader productExcelReader,
            IFileLoadErrorRepository fileLoadErrorRepository,
            IProcessedProductRepository processedProductRepository,
            IFileLoadNotificationOutbox notificationOutbox)
        {
            _fileLoadRepository = fileLoadRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _productExcelReader = productExcelReader;
            _fileLoadErrorRepository = fileLoadErrorRepository;
            _processedProductRepository = processedProductRepository;
            _notificationOutbox = notificationOutbox;
        }

        public async Task HandleAsync(
            ProcessFileRequested message,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (message.FileLoadId <= 0)
            {
                throw new ArgumentException(
                    "El identificador de la carga debe ser mayor que cero.",
                    nameof(message));
            }

            var fileLoad = await _fileLoadRepository.GetByIdAsync(
                message.FileLoadId,
                cancellationToken);

            if (fileLoad is null)
            {
                throw new InvalidOperationException(
                    $"No existe la carga con ID {message.FileLoadId}.");
            }

            if (fileLoad.Status is FileLoadStatus.Finished
                or FileLoadStatus.Notified
                or FileLoadStatus.Rejected
                or FileLoadStatus.Failed)
            {
                return;
            }

            if (fileLoad.Status == FileLoadStatus.Loaded)
            {
                await FinishFileLoadAsync(fileLoad, cancellationToken);
                return;
            }

            if (fileLoad.Status != FileLoadStatus.Pending &&
                fileLoad.Status != FileLoadStatus.InProcess)
            {
                throw new InvalidOperationException(
                    $"La carga {fileLoad.Id} no puede procesarse " +
                    $"en estado {fileLoad.Status}.");
            }

            if (string.IsNullOrWhiteSpace(fileLoad.FilePath))
            {
                throw new InvalidOperationException(
                    $"La carga {fileLoad.Id} no tiene un archivo asignado.");
            }

            await using var content = await _fileStorage.OpenReadAsync(
                                                                fileLoad.FilePath,
                                                                cancellationToken);

            if (fileLoad.Status == FileLoadStatus.Pending)
            {
                var previousStatus = fileLoad.Status;

                fileLoad.StartProcessing();

                var statusHistory = new FileLoadStatusHistory(
                    fileLoadId: fileLoad.Id,
                    previousStatus: previousStatus,
                    newStatus: fileLoad.Status,
                    description: "Se inició el procesamiento del archivo.");

                await _statusHistoryRepository.AddAsync(
                    statusHistory,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            try
            {
                var rowCount = 0;
                string? period = null;

                var productCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var products = new List<ProcessedProduct>();

                var rowNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var row in _productExcelReader.ReadRows(
                    content,
                    cancellationToken))
                {
                    rowCount++;

                    if (string.IsNullOrWhiteSpace(row.Period))
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PERIOD_REQUIRED",
                            $"La fila {row.RowNumber} no tiene periodo.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    var rowPeriod = row.Period.Trim();

                    if (rowPeriod.Length > 20)
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PERIOD_TOO_LONG",
                            $"La fila {row.RowNumber} tiene un periodo " +
                            "que supera los 20 caracteres.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    if (period is null)
                    {
                        period = rowPeriod;

                        if (fileLoad.Period is null)
                        {
                            fileLoad.AssignPeriod(period);
                        }
                        else if (!string.Equals(
                            fileLoad.Period,
                            period,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            await RejectFileLoadAsync(
                                fileLoad,
                                "PERIOD_MISMATCH",
                                $"El periodo '{period}' del archivo no coincide " +
                                $"con el periodo '{fileLoad.Period}' registrado en la carga.",
                                row.RowNumber,
                                cancellationToken);

                            return;
                        }

                        if (fileLoad.Period is null)
                        {
                            fileLoad.AssignPeriod(period);
                        }
                        else if (!string.Equals(
                            fileLoad.Period,
                            period,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            await RejectFileLoadAsync(
                                fileLoad,
                                "PERIOD_MISMATCH",
                                $"El periodo '{period}' del archivo no coincide " +
                                $"con el periodo '{fileLoad.Period}' registrado en la carga.",
                                row.RowNumber,
                                cancellationToken);

                            return;
                        }
                    }
                    else if (!string.Equals(
                        period,
                        rowPeriod,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PERIOD_MISMATCH",
                            $"La fila {row.RowNumber} tiene el periodo '{rowPeriod}', " +
                            $"pero la carga corresponde al periodo '{period}'.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(row.ProductCode))
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PRODUCT_CODE_REQUIRED",
                            $"La fila {row.RowNumber} no tiene código de producto.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    var productCode = row.ProductCode.Trim();

                    if (productCode.Length > 50)
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PRODUCT_CODE_TOO_LONG",
                            $"La fila {row.RowNumber} tiene un código de producto " +
                            "que supera los 50 caracteres.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    if (!productCodes.Add(productCode))
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PRODUCT_CODE_DUPLICATED",
                            $"La fila {row.RowNumber} tiene el código de producto " +
                            $"'{productCode}', que ya apareció en otra fila del Excel.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    var productName = string.IsNullOrWhiteSpace(row.ProductName)
                        ? ProductImportDefaultValues.ProductName
                        : row.ProductName.Trim();

                    var description = string.IsNullOrWhiteSpace(row.Description)
                        ? ProductImportDefaultValues.Description
                        : row.Description.Trim();

                    if (productName.Length > 200)
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PRODUCT_NAME_TOO_LONG",
                            $"La fila {row.RowNumber} tiene un nombre de producto " +
                            "que supera los 200 caracteres.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    if (description.Length > 2000)
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PRODUCT_DESCRIPTION_TOO_LONG",
                            $"La fila {row.RowNumber} tiene una descripción " +
                            "que supera los 2000 caracteres.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    var price = ProductImportDefaultValues.Price;

                    if (!string.IsNullOrWhiteSpace(row.Price))
                    {
                        if (!decimal.TryParse(
                            row.Price,
                            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture,
                            out price))
                        {
                            await RejectFileLoadAsync(
                                fileLoad,
                                "PRODUCT_PRICE_INVALID",
                                $"La fila {row.RowNumber} tiene un precio inválido. " +
                                "Debe ser numérico y usar punto como separador decimal.",
                                row.RowNumber,
                                cancellationToken);

                            return;
                        }

                        if (price < -9999999999999999.99m ||
                            price > 9999999999999999.99m ||
                            decimal.Round(price, 2) != price)
                        {
                            await RejectFileLoadAsync(
                                fileLoad,
                                "PRODUCT_PRICE_OUT_OF_RANGE",
                                $"La fila {row.RowNumber} tiene un precio que excede " +
                                "los 16 dígitos enteros o contiene más de 2 decimales significativos.",
                                row.RowNumber,
                                cancellationToken);

                            return;
                        }
                    }

                    var stock = ProductImportDefaultValues.Stock;

                    if (!string.IsNullOrWhiteSpace(row.Stock) &&
                        !int.TryParse(
                            row.Stock,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out stock))
                    {
                        await RejectFileLoadAsync(
                            fileLoad,
                            "PRODUCT_STOCK_INVALID",
                            $"La fila {row.RowNumber} tiene un stock inválido. " +
                            "Debe ser un número entero.",
                            row.RowNumber,
                            cancellationToken);

                        return;
                    }

                    var product = new ProcessedProduct(
                                            fileLoadId: fileLoad.Id,
                                            period: period!,
                                            productCode: productCode,
                                            productName: productName,
                                            description: description,
                                            price: price,
                                            stock: stock);

                    products.Add(product);
                    rowNumbers.Add(productCode, row.RowNumber);
                }

                if (rowCount == 0)
                {
                    await RejectFileLoadAsync(
                        fileLoad,
                        "FILE_WITHOUT_DATA",
                        "El Excel no contiene filas con datos para procesar.",
                        null,
                        cancellationToken);

                    return;
                }

                var existingProductCodes =
                    await _processedProductRepository.GetExistingProductCodesAsync(
                        productCodes,
                        cancellationToken);

                var existingCodes = existingProductCodes.ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

                var existingCount = 0;

                foreach (var product in products.Where(
                    product => existingCodes.Contains(product.ProductCode)))
                {
                    existingCount++;

                    await _fileLoadErrorRepository.AddAsync(
                        new FileLoadError(
                            fileLoadId: fileLoad.Id,
                            code: "PRODUCT_CODE_ALREADY_EXISTS",
                            message: $"Existente: el código de producto '{product.ProductCode}' " +
                                "ya está registrado y no se insertó nuevamente.",
                            rowNumber: rowNumbers[product.ProductCode]),
                        cancellationToken);
                }

                products.RemoveAll(
                    product => existingCodes.Contains(product.ProductCode));


                await _processedProductRepository.AddRangeAsync(
                    products,
                    cancellationToken);

                var statusBeforeLoaded = fileLoad.Status;

                fileLoad.MarkAsLoaded();

                var loadedHistory = new FileLoadStatusHistory(
                    fileLoadId: fileLoad.Id,
                    previousStatus: statusBeforeLoaded,
                    newStatus: fileLoad.Status,
                    description: $"Se cargaron {products.Count} productos correctamente. " + $"Se omitieron {existingCount} productos existentes.");

                await _statusHistoryRepository.AddAsync(
                    loadedHistory,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await FinishFileLoadAsync(fileLoad, cancellationToken);
            }
            catch (InvalidDataException exception)
            {
                await RejectFileLoadAsync(
                    fileLoad,
                    "INVALID_EXCEL_STRUCTURE",
                    exception.Message,
                    null,
                    cancellationToken);

                return;
            }

        }

        private async Task RejectFileLoadAsync(
            FileLoad fileLoad,
            string errorCode,
            string errorMessage,
            int? rowNumber,
            CancellationToken cancellationToken)
        {
            var previousStatus = fileLoad.Status;

            fileLoad.Reject();

            var error = new FileLoadError(
                fileLoadId: fileLoad.Id,
                code: errorCode,
                message: errorMessage,
                rowNumber: rowNumber);

            await _fileLoadErrorRepository.AddAsync(
                error,
                cancellationToken);

            var statusHistory = new FileLoadStatusHistory(
                fileLoadId: fileLoad.Id,
                previousStatus: previousStatus,
                newStatus: fileLoad.Status,
                description: errorMessage);

            await _statusHistoryRepository.AddAsync(
                statusHistory,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task FinishFileLoadAsync(
            FileLoad fileLoad,
            CancellationToken cancellationToken)
        {
            var previousStatus = fileLoad.Status;

            fileLoad.FinishProcessing();

            var statusHistory = new FileLoadStatusHistory(
                fileLoadId: fileLoad.Id,
                previousStatus: previousStatus,
                newStatus: fileLoad.Status,
                description: "Se finalizó el procesamiento del archivo.");

            await _statusHistoryRepository.AddAsync(
                statusHistory,
                cancellationToken);

            await _notificationOutbox.AddAsync(
                new FileLoadNotificationRequested(fileLoad.Id),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }


    }
}
