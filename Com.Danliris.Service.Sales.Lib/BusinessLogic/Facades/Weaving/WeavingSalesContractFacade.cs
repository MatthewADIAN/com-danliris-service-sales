﻿using Com.Danliris.Service.Sales.Lib.BusinessLogic.Interface.Weaving;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.Weaving;
using Com.Danliris.Service.Sales.Lib.Models.Weaving;
using Com.Danliris.Service.Sales.Lib.Services;
using Com.Danliris.Service.Sales.Lib.Utilities;
using Com.Danliris.Service.Sales.Lib.Utilities.BaseInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.Weaving
{
    public class WeavingSalesContractFacade : IWeavingSalesContract
    {
        private readonly SalesDbContext DbContext;
        private readonly DbSet<WeavingSalesContractModel> DbSet;
        private IdentityService IdentityService;
        private WeavingSalesContractLogic WeavingSalesContractLogic;

        public WeavingSalesContractFacade(IServiceProvider serviceProvider, SalesDbContext dbContext)
        {
            this.DbContext = dbContext;
            this.DbSet = this.DbContext.Set<WeavingSalesContractModel>();
            this.IdentityService = serviceProvider.GetService<IdentityService>();
            this.WeavingSalesContractLogic = serviceProvider.GetService<WeavingSalesContractLogic>();
        }

        public async Task<int> CreateAsync(WeavingSalesContractModel model)
        {
            do
            {
                model.Code = CodeGenerator.Generate();
            }
            while (this.DbSet.Any(d => d.Code.Equals(model.Code)));

            var data = await CustomCodeGenerator(model);
            WeavingSalesContractLogic.Create(data);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<WeavingSalesContractModel> ReadByIdAsync(int id)
        {
            return await WeavingSalesContractLogic.ReadByIdAsync(id);
        }

        public async Task<int> UpdateAsync(int id, WeavingSalesContractModel model)
        {
            WeavingSalesContractLogic.UpdateAsync(id, model);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            await WeavingSalesContractLogic.DeleteAsync(id);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<WeavingSalesContractModel> CustomCodeGenerator(WeavingSalesContractModel Model)
        {

            string type = Model.BuyerType.ToLower() == "export" ? "WVE" : "WVL";

            var lastData = await this.DbSet.Where(w => w.BuyerType == Model.BuyerType).OrderByDescending(o => o.CreatedUtc).FirstOrDefaultAsync();

            DateTime Now = DateTime.Now;
            string Year = Now.ToString("yy");
            string month = Now.ToString("mm");

            if (lastData == null)
            {
                Model.AutoIncrementNumber = 1;
                string Number = Model.AutoIncrementNumber.ToString().PadLeft(4, '0');
                Model.SalesContractNo = $"{Number}{type}{Number}{month}{Year}";
            }
            else
            {
                if (lastData.CreatedUtc.Year < Now.Year)
                {
                    Model.AutoIncrementNumber = 1;
                    string Number = Model.AutoIncrementNumber.ToString().PadLeft(4, '0');
                    Model.SalesContractNo = $"{Number}{type}{Number}{month}{Year}";
                }
                else
                {
                    Model.AutoIncrementNumber = lastData.AutoIncrementNumber + 1;
                    string Number = Model.AutoIncrementNumber.ToString().PadLeft(4, '0');
                    Model.SalesContractNo = $"{Number}{type}{Number}{month}{Year}";
                }
            }

            return Model;
        }

        public ReadResponse<WeavingSalesContractModel> Read(int page, int size, string order, List<string> select, string keyword, string filter)
        {
            return WeavingSalesContractLogic.Read(page, size, order, select, keyword, filter);
        }
    }
}
