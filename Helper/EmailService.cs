
//Mail duoc xac thuc danh tinh thi moi nhan duoc email. Mailgun k cho gui nhung mail rac chua duoc xac thuc
using RestSharp;
using System.Text;

public class EmailService
{
    private readonly string apiKey;
    private readonly string domain;

    public EmailService()
    {
        this.apiKey = "d707105a3805294a25d5d9ff50ffb065-1900dca6-224b0e9a";
        this.domain = "sandbox5cdb0913d81245c485e4b3a6a12d8a2d.mailgun.org";
    }
    public bool SendActivationEmail(string toEmail, string activationLink)
    {
        try
        {
            var client = new RestClient("https://api.mailgun.net/v3");
            var request = new RestRequest();
            request.Resource = $"{domain}/messages";
            request.AddParameter("from", "thaicong1995@gmail.com");
            request.AddParameter("to", toEmail);
            request.AddParameter("subject", "Activate Your Account");
            request.AddParameter("html", $"Click the link to activate your account: {activationLink}");
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes("api:" + apiKey)));
            request.Method = Method.Post;

            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                Console.WriteLine($"Password reset email sent to {toEmail} successfully.");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to send password reset email to {toEmail}. Error: {response.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            // Xử lý lỗi xảy ra trong quá trình gửi email.
            Console.WriteLine($"An error occurred while sending password reset email to {toEmail}. Error: {ex.Message}");
            return false;
        }
    }

    public bool SendPasswordResetEmail(string toEmail, string resetLink)
    {
        try
        {
            var client = new RestClient("https://api.mailgun.net/v3");
            var request = new RestRequest();
            request.Resource = $"{domain}/messages";
            request.AddParameter("from", "thaicong1995@gmail.com");
            request.AddParameter("to", toEmail);
            request.AddParameter("subject", "Reset Your Password");
            request.AddParameter("html", $"Click the link to reset your password: {resetLink}");
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes("api:" + apiKey)));
            request.Method = Method.Post;

            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                Console.WriteLine($"Password reset email sent to {toEmail} successfully.");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to send password reset email to {toEmail}. Error: {response.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            // Xử lý lỗi xảy ra trong quá trình gửi email.
            Console.WriteLine($"An error occurred while sending password reset email to {toEmail}. Error: {ex.Message}");
            return false;
        }
    }

}
