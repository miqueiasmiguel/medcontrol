using MedControl.Application.Common.Interfaces;
using Resend;

namespace MedControl.Infrastructure.Auth;

internal sealed class EmailService(IResend resend) : IEmailService
{
    private const string From = "noreply@fundamentoerp.com.br";

    public async Task SendMagicLinkAsync(string email, string magicLink, CancellationToken ct = default)
    {
        var message = new EmailMessage();
        message.From = From;
        message.To.Add(email);
        message.Subject = "Seu link de acesso ao MedControl";
        message.HtmlBody = $$"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"></head>
            <body style="margin:0;padding:0;background-color:#F8F9FA;font-family:'Inter',-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#F8F9FA;padding:40px 16px;">
                <tr><td align="center">
                  <table width="560" cellpadding="0" cellspacing="0" style="max-width:560px;width:100%;background-color:#FFFFFF;border-radius:12px;overflow:hidden;box-shadow:0 4px 6px -1px rgba(15,26,64,0.08),0 2px 4px -1px rgba(15,26,64,0.04);">

                    <!-- Header -->
                    <tr>
                      <td style="background-color:#0F1A40;padding:32px;text-align:center;">
                        <table cellpadding="0" cellspacing="0" style="margin:0 auto;">
                          <tr><td style="width:52px;height:52px;background-color:#F97316;border-radius:10px;text-align:center;vertical-align:middle;">
                            <span style="color:#FFFFFF;font-size:26px;font-weight:800;line-height:52px;display:block;">M</span>
                          </td></tr>
                          <tr><td style="padding-top:12px;">
                            <span style="color:#FFFFFF;font-size:18px;font-weight:600;letter-spacing:-0.01em;">MedControl</span>
                          </td></tr>
                        </table>
                      </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                      <td style="padding:40px 48px 32px;">
                        <h1 style="margin:0 0 8px;font-size:22px;font-weight:700;color:#0D0F12;letter-spacing:-0.02em;">Seu link de acesso</h1>
                        <p style="margin:0 0 8px;font-size:15px;color:#495057;line-height:1.6;">
                          Clique no botão abaixo para entrar no MedControl. O link é de <strong style="color:#0D0F12;">uso único</strong> e expira em <strong style="color:#0D0F12;">15 minutos</strong>.
                        </p>
                        <p style="margin:0 0 32px;font-size:14px;color:#868E96;line-height:1.5;">
                          Se você não solicitou este link, ignore este e-mail com segurança.
                        </p>

                        <!-- CTA Button -->
                        <table cellpadding="0" cellspacing="0" style="margin:0 0 32px;">
                          <tr><td style="background-color:#F97316;border-radius:8px;">
                            <a href="{{magicLink}}" style="display:inline-block;padding:14px 32px;color:#FFFFFF;font-size:15px;font-weight:600;text-decoration:none;letter-spacing:-0.01em;">
                              Entrar no MedControl
                            </a>
                          </td></tr>
                        </table>

                        <!-- Fallback link -->
                        <p style="margin:0;font-size:12px;color:#ADB5BD;line-height:1.6;">
                          Se o botão não funcionar, copie e cole o endereço abaixo no navegador:<br>
                          <a href="{{magicLink}}" style="color:#F97316;word-break:break-all;text-decoration:none;">{{magicLink}}</a>
                        </p>
                      </td>
                    </tr>

                    <!-- Divider -->
                    <tr><td style="padding:0 48px;"><div style="height:1px;background-color:#E9ECEF;"></div></td></tr>

                    <!-- Footer -->
                    <tr>
                      <td style="padding:20px 48px 28px;text-align:center;">
                        <p style="margin:0;font-size:12px;color:#ADB5BD;line-height:1.6;">
                          Este e-mail foi enviado por <strong style="color:#868E96;">MedControl</strong>.<br>
                          Você está recebendo porque uma solicitação de acesso foi feita com este endereço.
                        </p>
                      </td>
                    </tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        await resend.EmailSendAsync(message, ct);
    }

    public async Task SendInvitationAsync(string toEmail, string inviteLink, CancellationToken ct = default)
    {
        var message = new EmailMessage();
        message.From = From;
        message.To.Add(toEmail);
        message.Subject = "Você foi convidado para o MedControl";
        message.HtmlBody = $$"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"></head>
            <body style="margin:0;padding:0;background-color:#F8F9FA;font-family:'Inter',-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#F8F9FA;padding:40px 16px;">
                <tr><td align="center">
                  <table width="560" cellpadding="0" cellspacing="0" style="max-width:560px;width:100%;background-color:#FFFFFF;border-radius:12px;overflow:hidden;box-shadow:0 4px 6px -1px rgba(15,26,64,0.08),0 2px 4px -1px rgba(15,26,64,0.04);">

                    <!-- Header -->
                    <tr>
                      <td style="background-color:#0F1A40;padding:32px;text-align:center;">
                        <table cellpadding="0" cellspacing="0" style="margin:0 auto;">
                          <tr><td style="width:52px;height:52px;background-color:#F97316;border-radius:10px;text-align:center;vertical-align:middle;">
                            <span style="color:#FFFFFF;font-size:26px;font-weight:800;line-height:52px;display:block;">M</span>
                          </td></tr>
                          <tr><td style="padding-top:12px;">
                            <span style="color:#FFFFFF;font-size:18px;font-weight:600;letter-spacing:-0.01em;">MedControl</span>
                          </td></tr>
                        </table>
                      </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                      <td style="padding:40px 48px 32px;">
                        <h1 style="margin:0 0 8px;font-size:22px;font-weight:700;color:#0D0F12;letter-spacing:-0.02em;">Você foi convidado</h1>
                        <p style="margin:0 0 8px;font-size:15px;color:#495057;line-height:1.6;">
                          Você recebeu um convite para acessar o <strong style="color:#0D0F12;">MedControl</strong>, a plataforma de gestão médica simplificada.
                        </p>
                        <p style="margin:0 0 32px;font-size:15px;color:#495057;line-height:1.6;">
                          Clique no botão abaixo para criar sua conta e começar. O link expira em <strong style="color:#0D0F12;">48 horas</strong>.
                        </p>

                        <!-- CTA Button -->
                        <table cellpadding="0" cellspacing="0" style="margin:0 0 32px;">
                          <tr><td style="background-color:#F97316;border-radius:8px;">
                            <a href="{{inviteLink}}" style="display:inline-block;padding:14px 32px;color:#FFFFFF;font-size:15px;font-weight:600;text-decoration:none;letter-spacing:-0.01em;">
                              Aceitar convite
                            </a>
                          </td></tr>
                        </table>

                        <!-- Fallback link -->
                        <p style="margin:0;font-size:12px;color:#ADB5BD;line-height:1.6;">
                          Se o botão não funcionar, copie e cole o endereço abaixo no navegador:<br>
                          <a href="{{inviteLink}}" style="color:#F97316;word-break:break-all;text-decoration:none;">{{inviteLink}}</a>
                        </p>
                      </td>
                    </tr>

                    <!-- Divider -->
                    <tr><td style="padding:0 48px;"><div style="height:1px;background-color:#E9ECEF;"></div></td></tr>

                    <!-- Footer -->
                    <tr>
                      <td style="padding:20px 48px 28px;text-align:center;">
                        <p style="margin:0;font-size:12px;color:#ADB5BD;line-height:1.6;">
                          Se você não esperava este convite, ignore este e-mail com segurança.<br>
                          Este e-mail foi enviado por <strong style="color:#868E96;">MedControl</strong>.
                        </p>
                      </td>
                    </tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        await resend.EmailSendAsync(message, ct);
    }
}
