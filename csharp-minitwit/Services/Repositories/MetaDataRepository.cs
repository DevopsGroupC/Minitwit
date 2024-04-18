using System.Diagnostics;

using csharp_minitwit.Models.Entities;
using csharp_minitwit.Services.Interfaces;
using csharp_minitwit.Utils;

using Microsoft.EntityFrameworkCore;

namespace csharp_minitwit.Services.Repositories
{
    public class MetaDataRepository(MinitwitContext dbContext) : IMetaDataRepository
    {
        const int Id = 1;

        public async Task<int> GetLatestAsync()
        {
            var metaData = await dbContext.MetaData.FindAsync(Id);
            return metaData?.Latest ?? -1;
        }

        public async Task<bool> SetLatestAsync(int latest)
        {
            var metaData = await dbContext.MetaData.FindAsync(Id);
            if (metaData == null)
            {
                metaData = new MetaData { Id = Id, Latest = latest };
                dbContext.MetaData.Add(metaData);
            }
            else
            {
                metaData.Latest = latest;
            }
            var result = await dbContext.SaveChangesAsync();
            return result > 0;
        }

    }
}