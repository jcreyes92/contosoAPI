using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;
using Repository.Extensions.Utility;
using System.Linq.Dynamic.Core;

namespace Repository.Extensions
{
    public static class RepositoryCompanyExtensions
    {

        public static IQueryable<Company> Search(this IQueryable<Company> companies, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return companies;

            var lowerCaseTerm = searchTerm.Trim().ToLower();

            return companies.Where(e => e.Name.ToLower().Contains(lowerCaseTerm));
        }

        public static IQueryable<Company> Sort(this IQueryable<Company> companies, string orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return null;

            var orderQuery = OrderQueryBuilder.CreateOrderQuery<Company>(orderByQueryString);

            if (string.IsNullOrWhiteSpace(orderQuery))
                return null;

            return companies.OrderBy(orderQuery);
        }
    }
}
