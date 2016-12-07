/* http://www.zkea.net/ Copyright 2016 ZKEASOFT http://www.zkea.net/licenses */
using System.Collections.Generic;
using System.Linq;
using Easy.Extend;
using Easy.RepositoryPattern;
using Easy.Mvc;
using Easy.Mvc.ValueProvider;
using Easy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;

namespace ZKEACMS.Theme
{
    public class ThemeService : ServiceBase<ThemeEntity, CMSDbContext>, IThemeService
    {
        private readonly ICookie _cookie;
        private const string PreViewCookieName = "PreViewTheme";
        private readonly IHttpContextAccessor _httpContextAccessor;



        public ThemeService(ICookie cookie, IHttpContextAccessor httpContextAccessor, IApplicationContext applicationContext)
            : base(applicationContext)
        {
            _cookie = cookie;
            _httpContextAccessor = httpContextAccessor;
        }
        public override DbSet<ThemeEntity> CurrentDbSet
        {
            get
            {
                return DbContext.Theme;
            }
        }
        public void SetPreview(string id)
        {
            _cookie.SetValue(PreViewCookieName, id, true, true);
        }

        public void CancelPreview()
        {
            _cookie.GetValue<string>(PreViewCookieName, true);
        }

        public ThemeEntity GetCurrentTheme()
        {
            var id = _cookie.GetValue<string>(PreViewCookieName);
            ThemeEntity theme = null;
            if (id.IsNotNullAndWhiteSpace() && (_httpContextAccessor.HttpContext.User?.Identity.IsAuthenticated ?? false))
            {
                theme = Get(id);
                if (theme != null)
                {
                    theme.IsPreView = true;
                }
            }
            return theme ?? Get(m => m.IsActived == true).FirstOrDefault();
        }


        public void ChangeTheme(string id)
        {
            if (id.IsNullOrEmpty()) return;

            var theme = Get(id);
            if (theme != null)
            {
                var otherTheme = new ThemeEntity { IsActived = false };

                CurrentDbSet.Attach(otherTheme);
                DbContext.Entry(otherTheme).Property(m => m.IsActived).IsModified = true;
                DbContext.SaveChanges();

                theme.IsActived = true;
                Update(theme);
            }
        }
    }
}