using AutoMapper;
using CryptoWise.API.Entities;
using CryptoWise.Shared.Account;

namespace CryptoWise.API.Helpers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Account, AccountResponse>();

        CreateMap<Account, AuthenticateResponse>();

        CreateMap<RegisterRequest, Account>();

        CreateMap<CreateRequest, Account>();
        
        CreateMap<UpdateRequest, Account>()
            .ForAllMembers(x => x.Condition(
                (src, dest, prop) =>
                {
                    if (prop == null) return false;
                    if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;
                    
                    if (x.DestinationMember.Name == "Role" && src.Role == null) return false;

                    return true;
                }
            ));
    }
}