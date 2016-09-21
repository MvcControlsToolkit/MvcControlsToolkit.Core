using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
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
        public ContextualizedHelpers(ViewContext context, IHtmlHelper html, IPrincipal user, IViewComponentHelper component, IUrlHelper urlHelper, IStringLocalizerFactory localizerFactory = null)
        {
            _html = html;
            _component = component;
            this._context = context;
            _htmlIsContextualized = true;
            _componentIsContextualized = true;
            _user = user;
            _urlHelper = urlHelper;
            LocalizerFactory = localizerFactory;
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
    }
}
