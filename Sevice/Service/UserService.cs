
//Công việc cần xử lý : Xử lý token khi log out. - mã token hết hạn lập tức không còn hiệu lực.
                    // - khi đăng nhập lần 2 mã token lần trước đó không còn hiệu lực.
                    // - Kích hoạt tài khoản đăng ký qua email - Đổi password xac thực qua email.
using WebApi.Dto;
using WebApi.Models;
using WebApi.Models.Enum;
using WebApi.MyDbContext;
using WebApi.Sevice.Interface;
using WebApi.TokenConfig;

namespace WebApi.Sevice.Service
{
    public class UserService : IUserService
    {
        private readonly Token _token;

        private readonly MyDb _myDb;
        private readonly IShopService _iShopService;
        private readonly IWalletService _iWalletService;
        private readonly EmailService _emailService;

        public UserService(MyDb myDb, Token token, IShopService shopService, IWalletService walletService, EmailService emailService)
        {
            _myDb = myDb;
            _token = token;
            _iShopService = shopService;
            _iWalletService = walletService;
            _emailService = emailService;
        }

      

        public List<User> GetUsers()
        {
            try
            {
                List<User> users = _myDb.Users.ToList();

                return users;
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred: {e.Message}");
            }
        }

        public User GetUserByID(int UserId)
        {
            return _myDb.Users.FirstOrDefault(s => s.Id == UserId);
        }

        //Chưa test trường hợp gửi mail thất bại (Nếu đăng ký thành công gửi mail fail hoặc hết hạn -- đã xong.)
        // Login xác thực active 1 lần nữa.
        public User RegisterUser(User user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "Not enough information has been entered.");
                }

                // Mã hóa mật khẩu và thêm người dùng vào cơ sở dữ liệu
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user._userStatus = UserStatus.Inactive;

                string activationToken = Guid.NewGuid().ToString();
                user.ActivationToken = activationToken;

                // Thêm người dùng vào cơ sở dữ liệu trước khi gửi email xác nhận
                _myDb.Users.Add(user);
                _myDb.SaveChanges();

                // Tạo liên kết kích hoạt
                string activationLink = "https://localhost:7212/api/activate?activationToken=" + activationToken;
                _emailService.SendActivationEmail(user.Email, activationLink);
                // Gửi liên kết kích hoạt cho người dùng 

                _iShopService.CreateShopForUser(user);
                _iWalletService.CreateWalletForUser(user);

                return user;
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred: {e.Message}");
            }
        }

        public bool ActivateUser(string activationToken)
        {
            var user = _myDb.Users.FirstOrDefault(u => u.ActivationToken == activationToken);

            if (user != null)
            {
                user._userStatus = UserStatus.Active;
                user.ActivationToken = null;
                _myDb.SaveChanges();
                return true;
            }

            return false;
        }

        public bool SendPasswordResetEmail(string email)
        {
            var user = _myDb.Users.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                string resetToken = Guid.NewGuid().ToString();
                user.ActivationToken = resetToken;

                _myDb.SaveChanges();

                string resetLink = "https://localhost:7212/api/ResetPasswordForEmail/Reset-Password?token=" + resetToken;
                _emailService.SendPasswordResetEmail(user.Email, resetLink);

                return true;
            }

            return false;
        }


        public User LoginUser(UserDto userDto)
        {
            try
            {
                // Tìm người dùng trong cơ sở dữ liệu bằng Email
                User user = _myDb.Users.SingleOrDefault(u => u.Email == userDto.Email);

                if (user == null)
                {
                    throw new Exception("User not found!");
                }

                if (!BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password))
                {
                    throw new Exception("Wrong password!");
                }

                if (user._userStatus == UserStatus.Inactive)
                {
                    string newActivationToken = Guid.NewGuid().ToString();
                    user.ActivationToken = newActivationToken;
                    _myDb.SaveChanges();

                    // Gửi email kích hoạt mới
                    string activationLink = "https://localhost:7212/api/activate?activationToken=" + newActivationToken;
                    _emailService.SendActivationEmail(user.Email, activationLink);
                    throw new Exception("Account has not been activated. Please check your email to activate your account.");
                }

               
                    var existingToken = _myDb.AccessTokens.FirstOrDefault(t => t.UserID == user.Id && t.statusToken == StatusToken.Valid);

                    if (existingToken != null)
                    {
                        // Nếu có token hiện tại, sẽ ghi đè lên token hiện tại
                        var token = _token.CreateToken(user);

                        if (string.IsNullOrEmpty(token))
                        {
                            throw new Exception("Failed to create a token.");
                        }

                        existingToken.AccessToken = token; // Ghi đè token hiện tại
                        
                        // Cập nhật thời gian hết hạn cho token hiện tại (nếu cần)
                        existingToken.ExpirationDate = DateTime.Now.AddMinutes(30); 

                    }
                    else
                    {
                    // Nếu chưa có token, bạn sẽ tạo token mới và lưu vào cơ sở dữ liệu
                    // (Trong trường hợp không ghi đè thì set statusToken trước đó = 1) để thu hồi mã token trước
                    var token = _token.CreateToken(user);

                        if (string.IsNullOrEmpty(token))
                        {
                            throw new Exception("Failed to create a token.");
                        }

                        var accessToken = new AcessToken
                        {
                            UserID = user.Id,
                            AccessToken = token,
                            statusToken = StatusToken.Valid,
                            ExpirationDate = DateTime.Now.AddMinutes(30) // Ví dụ: đặt thời gian hết hạn là 3 phút sau
                        };


                        _myDb.AccessTokens.Add(accessToken);
                    }
                
                    // Lưu thay đổi vào cơ sở dữ liệu
                  _myDb.SaveChanges();
                  return user;
                
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred: {e.Message}");
            }
        }

        public void TokensAsExpired(int userId)
        {
            var userTokens = _myDb.AccessTokens.Where(t => t.UserID == userId && t.statusToken == StatusToken.Valid).ToList();
            foreach (var token in userTokens)
            {
                token.statusToken = StatusToken.Expired;
            }
            _myDb.SaveChanges();
        }

        public bool LogoutUser(int userId)
        {
            try
            {
                // Lấy danh sách các token của người dùng
                var userTokens = _myDb.AccessTokens.Where(t => t.UserID == userId && t.statusToken == StatusToken.Valid).ToList();

                foreach (var token in userTokens)
                {
                    var tokenValue = token.AccessToken; // Lấy giá trị của token từ cơ sở dữ liệu
                    var principal = _token.ValidateToken(tokenValue); // Xác thực token

                    if (principal != null)
                    {
                        // Token hợp lệ, bạn có thể đánh dấu token đã hết hạn
                        token.statusToken = StatusToken.Expired;
                    }
                }

                _myDb.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
