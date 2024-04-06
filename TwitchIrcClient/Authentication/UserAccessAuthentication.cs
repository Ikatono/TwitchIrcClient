using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.Authentication
{
    public record class UserAccessAuthentication(string Token, string ClientId);
}
