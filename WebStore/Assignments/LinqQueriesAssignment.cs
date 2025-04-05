using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebStore.Entities;

namespace WebStore.Assignments
{
    /// Additional tutorial materials https://dotnettutorials.net/lesson/linq-to-entities-in-entity-framework-core/

    // https://github.com/PullasorsaHUB/Oppimisteht-v-SQL.git

    /// <summary>
    /// This class demonstrates various LINQ query tasks 
    /// to practice querying an EF Core database.
    /// 
    /// ASSIGNMENT INSTRUCTIONS:
    ///   1. For each method labeled "TODO", write the necessary
    ///      LINQ query to return or display the required data.
    ///      
    ///   2. Print meaningful output to the console (or return
    ///      collections, as needed).
    ///      
    ///   3. Test each method by calling it from your Program.cs
    ///      or test harness.
    /// </summary>
    public class LinqQueriesAssignment
    {

         //TODO: Uncomment this code after generating the entity models

        private readonly WebStoreContext _dbContext;

        public LinqQueriesAssignment(WebStoreContext context)
        {
            _dbContext = context;
        }


        /// <summary>
        /// 1. List all customers in the database:
        ///    - Print each customer's full name (First + Last) and Email.
        /// </summary>
        public async Task Task01_ListAllCustomers()
        {
            // TODO: Write a LINQ query that fetches all customers
            //       and prints their names + emails to the console.
            // HINT: context.Customers
            
            var customers = await _dbContext.Customers
               // .AsNoTracking() // optional for read-only
               .ToListAsync();

            Console.WriteLine("=== TASK 01: List All Customers ===");

            foreach (var c in customers)
            {
                Console.WriteLine($"{c.FirstName} {c.LastName} - {c.Email}");
            }

            
        }

        /// <summary>
        /// 2. Fetch all orders along with:
        ///    - Customer Name
        ///    - Order ID
        ///    - Order Status
        ///    - Number of items in each order (the sum of OrderItems.Quantity)
        /// </summary>
        public async Task Task02_ListOrdersWithItemCount()
        {
            // TODO: Write a query to return all orders,
            //       along with the associated customer name, order status,
            //       and the total quantity of items in that order.

            // HINT: Use Include/ThenInclude or projection with .Select(...).
            //       Summing the quantities: order.OrderItems.Sum(oi => oi.Quantity).

            var ListOrder = await _dbContext.Orders
               .Include(o => o.Customer)
               .Include(o => o.OrderItems)
               .Select(o => new
               {
                   OrderId = o.OrderId,
                   CustomerName = o.Customer.FirstName + " " + o.Customer.LastName,
                   o.OrderStatus,
                   ItemCount = o.OrderItems.Sum(oi => oi.Quantity)
               })
                .ToListAsync();



            Console.WriteLine(" ");
            Console.WriteLine("=== TASK 02: List Orders With Item Count ===");

            foreach (var order in ListOrder)
            {
                Console.WriteLine($"Order #{order.OrderId} | {order.CustomerName} | Status: {order.OrderStatus}");
            }
        }

        /// <summary>
        /// 3. List all products (ProductName, Price),
        ///    sorted by price descending (highest first).
        /// </summary>
        public async Task Task03_ListProductsByDescendingPrice()
        {
            // TODO: Write a query to fetch all products and sort them
            //       by descending price.
            // HINT: context.Products.OrderByDescending(p => p.Price)
            var products = await _dbContext.Products
                .OrderByDescending(p => p.Price)
                .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 03: List Products By Descending Price ===");
            foreach(var product in products)
            {
                Console.WriteLine($"{product.ProductName} - ${product.Price}");
            }
        }

        /// <summary>
        /// 4. Find all "Pending" orders (order status = "Pending")
        ///    and display:
        ///      - Customer Name
        ///      - Order ID
        ///      - Order Date
        ///      - Total price (sum of unit_price * quantity - discount) for each order
        /// </summary>
        public async Task Task04_ListPendingOrdersWithTotalPrice()
        {
            // TODO: Write a query to fetch only PENDING orders,
            //       and calculate their total price.
            // HINT: The total can be computed from each OrderItem:
            //       (oi.UnitPrice * oi.Quantity) - oi.Discount

            var pendingOrder = await _dbContext.Orders
                .Where(o => o.OrderStatus == "Pending")
                .Include (o => o.Customer)
                .Include(o => o.OrderItems)
                .Select(o => new
                {
                    o.OrderId,
                    CustomerName = o.Customer.FirstName + " " + o.Customer.LastName,
                    o.OrderDate,
                    Total = o.OrderItems.Sum(oi => (oi.UnitPrice * oi.Quantity) - oi.Discount)
                })
                .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 04: List Pending Orders With Total Price ===");
            foreach (var order in pendingOrder)
            {
                Console.WriteLine($"Order #{order.OrderId} | {order.CustomerName} | Date: {order.OrderDate:d} | Total: ${order.Total:F2}");
            }
        }

        /// <summary>
        /// 5. List the total number of orders each customer has placed.
        ///    Output should show:
        ///      - Customer Full Name
        ///      - Number of Orders
        /// </summary>
        public async Task Task05_OrderCountPerCustomer()
        {
            // TODO: Write a query that groups by Customer,
            //       counting the number of orders each has.

            // HINT: 
            //  1) Join Orders and Customers, or
            //  2) Use the navigation (context.Orders or context.Customers),
            //     then group by customer ID or by the customer entity.

            var orderCounts = await _dbContext.Orders
                .GroupBy(o => o.Customer)
                .Select(g => new
                {
                    CustomerName = g.Key.FirstName + " " + g.Key.LastName,
                    OrderCount = g.Count()
                })
                .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 05: Order Count Per Customer ===");
            
            foreach(var order in orderCounts)
            {
                Console.WriteLine($"{order.CustomerName}: {order.OrderCount} orders");
            }
        }

        /// <summary>
        /// 6. Show the top 3 customers who have placed the highest total order value overall.
        ///    - For each customer, calculate SUM of (OrderItems * Price).
        ///      Then pick the top 3.
        /// </summary>
        public async Task Task06_Top3CustomersByOrderValue()
        {
            // TODO: Calculate each customer's total order value 
            //       using their Orders -> OrderItems -> (UnitPrice * Quantity - Discount).
            //       Sort descending and take top 3.

            // HINT: You can do this in a single query or multiple steps.
            //       One approach:
            //         1) Summarize each Order's total
            //         2) Summarize for each Customer
            //         3) Order by descending total
            //         4) Take(3)

            var topCustomers = await _dbContext.Customers
                .Select(c => new
                {
                    CustomerName = c.FirstName + " " + c.LastName,
                    TotalValue = c.Orders
                        .SelectMany(o => o.OrderItems)
                        .Sum(oi =>(oi.UnitPrice * oi.Quantity) -oi.Discount)
                })
                .OrderByDescending(x => x.TotalValue)
                .Take(3)
                .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 06: Top 3 Customers By Order Value ===");
            foreach (var item in topCustomers)
            {
                Console.WriteLine($"{item.CustomerName} - Total: ${item.TotalValue:F2}");
            }
        }

        /// <summary>
        /// 7. Show all orders placed in the last 30 days (relative to now).
        ///    - Display order ID, date, and customer name.
        /// </summary>
        public async Task Task07_RecentOrders()
        {
            // TODO: Filter orders to only those with OrderDate >= (DateTime.Now - 30 days).
            //       Output ID, date, and the customer's name.

            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var recentOrders = await _dbContext.Orders
                .Where(o => o.OrderDate >= thirtyDaysAgo)
                .Include(o => o.Customer)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    CustomerName = o.Customer.FirstName + " " + o.Customer.LastName,
                })
                .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 07: Recent Orders ===");
            foreach (var item in recentOrders)
            {
                Console.WriteLine($"Order #{item.OrderId} | {item.OrderDate:d} | {item.CustomerName}");
            }
        }

        /// <summary>
        /// 8. For each product, display how many total items have been sold
        ///    across all orders.
        ///    - Product name, total sold quantity.
        ///    - Sort by total sold descending.
        /// </summary>
        public async Task Task08_TotalSoldPerProduct()
        {
            // TODO: Group or join OrdersItems by Product.
            //       Summation of quantity.

            var sales = await _dbContext.OrderItems
                .GroupBy(oi => oi.Product)
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 08: Total Sold Per Product ===");
            foreach(var p in sales)
            {
                Console.WriteLine($"{p.ProductName} - Sold: {p.TotalSold}");
            }
        }

        /// <summary>
        /// 9. List any orders that have at least one OrderItem with a Discount > 0.
        ///    - Show Order ID, Customer name, and which products were discounted.
        /// </summary>
        public async Task Task09_DiscountedOrders()
        {
            // TODO: Identify orders with any OrderItem having (Discount > 0).
            //       Display order details, plus the discounted products.

            var discountedOrders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi => oi.Discount > 0))
                .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== Task 09: Discounted Orders ===");
            foreach( var o in discountedOrders)
            {
                Console.WriteLine($"Order #{o.OrderId} | {o.Customer.FirstName} {o.Customer.FirstName}");
                foreach(var item in o.OrderItems.Where(oi => oi.Discount > 0))
                {
                    Console.WriteLine($" - Discounted Product: {item.Product.ProductName} | Discount: ${item.Discount}");
                }
            }
        }

        /// <summary>
        /// 10. (Open-ended) Combine multiple joins or navigation properties
        ///     to retrieve a more complex set of data. For example:
        ///     - All orders that contain products in a certain category
        ///       (e.g., "Electronics"), including the store where each product
        ///       is stocked most. (Requires `Stocks`, `Store`, `ProductCategory`, etc.)
        ///     - Or any custom scenario that spans multiple tables.
        /// </summary>
        public async Task Task10_AdvancedQueryExample()
        {
            // TODO: Design your own complex query that demonstrates
            //       multiple joins or navigation paths. For example:
            //       - Orders that contain any product from "Electronics" category.
            //       - Then, find which store has the highest stock of that product.
            //       - Print results.

            // Here's an outline you could explore:
            // 1) Filter products by category name "Electronics"
            // 2) Find any orders that contain these products
            // 3) For each of those products, find the store with the max stock
            //    (requires .MaxBy(...) in .NET 6+ or custom code in older versions)
            // 4) Print a combined result

            // (Implementation is left as an exercise.)
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 10: Advanced Query Example ===");

            var orders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Categories)
                .ToListAsync();

            var electronicsOrders = orders
                .Where(o => o.OrderItems
                    .Any(oi => oi.Product.Categories
                        .Any(pc => pc.CategoryName == "Electronics")))
                .ToList();

            foreach (var order in electronicsOrders)
            {
                Console.WriteLine($"\nOrder #{order.OrderId} | Customer: {order.Customer.FirstName} {order.Customer.LastName}");

                foreach (var item in order.OrderItems)
                {
                    var product = item.Product;

                    bool isElectronics = product.Categories
                        .Any(pc => pc.CategoryName == "Electronics");

                    if (!isElectronics) continue;

                    var topStockStore = await _dbContext.Stocks
                        .Where(s => s.ProductId == product.ProductId)
                        .Include(s => s.Store)
                        .OrderByDescending(s => s.QuantityInStock)
                        .Select(s => new
                        {
                            s.QuantityInStock,
                            StoreName = s.Store.StoreName
                        })
                        .FirstOrDefaultAsync();

                    Console.WriteLine($" - Product: {product.ProductName} | Top Store: {topStockStore?.StoreName ?? "N/A"} ({topStockStore?.QuantityInStock ?? 0} units)");
                }
            }
        }
    }
}
