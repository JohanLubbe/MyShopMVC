using NewShop.Core.Contracts;
using NewShop.Core.Models;
using NewShop.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NewShop.Services
{
    public class BasketService : IBasketService
    {
        IRepository<Product> productContext;
        IRepository<Basket> basketContext;

        public const string BasketSessionName = "eCommerceBasket";

        public BasketService(IRepository<Product> ProductContext, IRepository<Basket> BasketContext)
        {
            this.basketContext = BasketContext;
            this.productContext = ProductContext;
        }

        // Load the basket
        private Basket GetBasket(HttpContextBase httpContext, bool createifNull)
        {
            HttpCookie cookie = httpContext.Request.Cookies.Get(BasketSessionName);

            Basket basket = new Basket();

            if (cookie != null)
            {
                string basketId = cookie.Value;
                if (!string.IsNullOrEmpty(basketId))
                {
                    basket = basketContext.Find(basketId);
                }
                if (createifNull == true)
                {
                    basket = CreateNewBasket(httpContext);
                }
            }
            else // Cookie is null
            {
                if (createifNull == true)
                {
                    basket = CreateNewBasket(httpContext);
                }
            }

            return (basket);
        }

        private Basket CreateNewBasket(HttpContextBase httpContext)
        {
            Basket basket = new Basket();
            basketContext.Insert(basket);
            basketContext.Commit();


            HttpCookie cookie = new HttpCookie(BasketSessionName);
            cookie.Value = basket.Id;
            cookie.Expires = DateTime.Now.AddDays(1);
            httpContext.Response.Cookies.Add(cookie);

            return (basket);
        }

        public void AddToBasket(HttpContextBase httpContext, string productId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                item = new BasketItem() { BasketId = basket.Id, ProductId = productId, Quantity = 1 };

                basket.BasketItems.Add(item);
            }
            else
            {
               item.Quantity = item.Quantity + 1;
            }

            basketContext.Commit();
        }

        public void RemoveFromBasket(HttpContextBase httpContext, string itemId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.Id == itemId); // Uses the basketID not the product ID

            if (item != null)
            {
                basket.BasketItems.Remove(item);
                basketContext.Commit();
            }
        }

        public List<BasketItemViewModel> GetBasketItems(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);
            if (basket != null)
            {
                var result = (from b in basket.BasketItems
                              join p in productContext.Collection() on b.ProductId equals p.Id
                              select new BasketItemViewModel()
                              {
                                  Id = b.Id,
                                  Quantity = b.Quantity,
                                  ProductName = p.Name,
                                  Image = p.Image,
                                  Price = p.Price
                              }
                              ).ToList();
                return result;
            }
            else
            {
                return new List<BasketItemViewModel>();
            }
        }

        public BasketSummaryViewModel GetBasketSummary(HttpContextBase httpcontext)
        {
            Basket basket = GetBasket(httpcontext, false);
            BasketSummaryViewModel model = new BasketSummaryViewModel(0, 0);

            if (basket != null)
            {
                // Declare an int that can be null add ?
                int? basketCount = (from item in basket.BasketItems
                                    select item.Quantity).Sum();

                decimal? basketTotal = (from item in basket.BasketItems
                                        join p in productContext.Collection() on item.ProductId equals p.Id
                                        select item.Quantity * p.Price).Sum();

                model.BasketCount = basketCount ?? 0; // Check if it is a null if it is 0 the value
                model.BasketTotal = basketTotal ?? decimal.Zero;

                return model;
            }
            else
            {
                return model;
            }
        }
    }
}
