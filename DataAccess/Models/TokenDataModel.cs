using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class TokenDataModel
    {
        /// <summary>
        /// This property contains the token content
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// This property contains the expiration date of the token
        /// </summary>
        public DateTime TokenExpiration { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserOfToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsActivationToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsResetPasswordToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsSessionToken { get; set; }
    }
}
