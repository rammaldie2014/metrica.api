using MailKit.Net.Smtp;
using MailKit.Security;
using Metrica.Application.Interfaces.Notifications;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Notifications
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            cancellationToken.ThrowIfCancellationRequested();

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _options.SenderName,
                    _options.SenderEmail));

            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            var security = _options.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _options.Host,
                _options.Port,
                security,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                await client.AuthenticateAsync(
                    _options.UserName,
                    _options.Password,
                    cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);

            await client.DisconnectAsync(
                quit: true,
                cancellationToken: cancellationToken);
        }
    }
}
