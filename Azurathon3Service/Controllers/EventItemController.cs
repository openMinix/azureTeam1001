using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using Azurathon3Service.DataObjects;
using Azurathon3Service.Models;

namespace Azurathon3Service.Controllers
{
    public class EventItemController : TableController<EventItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            Azurathon3Context context = new Azurathon3Context();
            DomainManager = new EntityDomainManager<EventItem>(context, Request, Services);
        }

        // GET tables/EventItem
        public IQueryable<EventItem> GetAllEventItem()
        {
            return Query(); 
        }

        // GET tables/EventItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<EventItem> GetEventItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/EventItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<EventItem> PatchEventItem(string id, Delta<EventItem> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/EventItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<IHttpActionResult> PostEventItem(EventItem item)
        {
            EventItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/EventItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteEventItem(string id)
        {
             return DeleteAsync(id);
        }

    }
}