using DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Reposetoriy
{
    public class UserRepo:GenaricRepo<App_User>, IUserRepo
    {
        public UserRepo(AppDbcontext context):base(context)
        { 
        
        
        
        
        }
    }
}
