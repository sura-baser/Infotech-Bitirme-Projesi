using Microsoft.AspNetCore.Identity;

namespace PastaneApp.Data.Identity;

public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        new() { Code = nameof(DefaultError), Description = "Beklenmeyen bir hata oluştu." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"Şifre en az {length} karakter olmalıdır." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Şifre en az bir özel karakter (örn. ! . *) içermelidir." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "Şifre en az bir rakam içermelidir." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "Şifre en az bir küçük harf içermelidir." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "Şifre en az bir büyük harf içermelidir." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Şifre en az {uniqueChars} farklı karakter içermelidir." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = "Bu e-posta adresi zaten kayıtlı." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = "Bu e-posta adresi zaten kayıtlı." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = "Geçerli bir e-posta adresi girin." };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = "Kullanıcı adı geçersiz karakterler içeriyor." };

    public override IdentityError PasswordMismatch() =>
        new() { Code = nameof(PasswordMismatch), Description = "Mevcut şifre hatalı." };
}
