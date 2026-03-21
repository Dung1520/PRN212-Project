using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AdminDAO _dao = new AdminDAO();

        public Admin? GetByEmail(string email)
            => _dao.GetByEmail(email);
    }
}
