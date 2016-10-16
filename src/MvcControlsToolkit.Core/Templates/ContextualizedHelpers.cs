using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;

namespace MvcControlsToolkit.Core.Templates
{
    public class ContextualizedHelpers
    {
        private IHtmlHelper _html;
        private bool _htmlIsContextualized;
        private IViewComponentHelper _component;
        private bool _componentIsContextualized;
        private ViewContext _context;
        private IUrlHelperFactory urlHelperFactory;
        private IUrlHelper _urlHelper;
        private HttpContext _httpContext;
        private IPrincipal _user;
        private IDictionary<object, IHtmlContent> cachedTemplateResult;
        public ContextualizedHelpers(ViewContext context, IHtmlHelper html, IHttpContextAccessor httpAccessor, IViewComponentHelper component, IUrlHelperFactory urlHelperFactory, IStringLocalizerFactory localizerFactory = null)
        {
            _html = html;
            _component = component;
            this._context = context;
            this.urlHelperFactory = urlHelperFactory;
            _httpContext=httpAccessor.HttpContext;
            _user = _httpContext.User;
            LocalizerFactory = localizerFactory;

        }
        public ContextualizedHelpers(ViewContext context, IHtmlHelper html, IPrincipal user, IViewComponentHelper component, IUrlHelper urlHelper, IStringLocalizerFactory localizerFactory)
        {
            _html = html;
            _component = component;
            this._context = context;
            _htmlIsContextualized = true;
            _componentIsContextualized = true;
            _user = user;
            _httpContext = context.HttpContext;
            _urlHelper = urlHelper;
            LocalizerFactory = localizerFactory;
        }
        public ContextualizedHelpers CreateChild(ViewContext context, IHtmlHelper html, IPrincipal user= null, IViewComponentHelper component= null, IUrlHelper urlHelper= null, IStringLocalizerFactory localizerFactory= null)
        {
            var res = new ContextualizedHelpers(context, html, user??_user, component??_component, urlHelper??_urlHelper, localizerFactory??LocalizerFactory);
            res.cachedTemplateResult = cachedTemplateResult;
            return res;
        }
        public void AddCachedTemplateResult(object template, IHtmlContent result)
        {
            if (cachedTemplateResult == null) cachedTemplateResult = new Dictionary<object, IHtmlContent>();
            cachedTemplateResult[template] = result;
        }
        public IHtmlContent GetCachedTemplateResult(object template)
        {
            if (cachedTemplateResult == null) return null;
            IHtmlContent res;
            if (cachedTemplateResult.TryGetValue(template, out res)) return res;
            else return null;
        }
        public IStringLocalizerFactory LocalizerFactory { get; private set; }
        public IHtmlHelper Html
        {
            get
            {
                if (!_htmlIsContextualized) (_html as IViewContextAware).Contextualize(_context);
                return _html;
            }
        }
        public ViewContext Context
        {
            get
            {
                return _context;
            }
        }
        public IViewComponentHelper Component
        {
            get
            {
                if (!_componentIsContextualized) (_component as IViewContextAware).Contextualize(_context);
                return _component;
            }
        }
        public IUrlHelper UrlHelper
        {
            get
            {
                if (_urlHelper == null) _urlHelper = urlHelperFactory.GetUrlHelper(_context);
                return _urlHelper;
            }
        }
        

        public IPrincipal User
        {
            get
            {
                return _user;
            }
        }

        public HttpContext CurrentHttpContext
        {
            get
            {
                return _httpContext;
            }
        }
    }
}
