using Application.Utils;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedShopAndProductsAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                try
                {
                    // --- 1. Tạo Shop Owner (Nếu chưa có) ---
                    var shopOwnerEmail = "cobon@example.com";
                    var shopOwner = await context.AppUsers.FirstOrDefaultAsync(u => u.Email == shopOwnerEmail);
                    if (shopOwner == null)
                    {
                        shopOwner = new AppUser
                        {
                            Id = Guid.NewGuid(),
                            FullName = "Chị Bốn", // Tên chủ shop
                            Email = shopOwnerEmail,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ShopPassword123!"), // Đặt mật khẩu an toàn!
                            Roles = new List<string> { "Shop" } // Gán quyền Shop ngay
                        };
                        await context.AppUsers.AddAsync(shopOwner);
                        await context.SaveChangesAsync(); // Lưu user trước để lấy ID
                        logger.LogInformation($"Đã tạo user chủ shop: {shopOwnerEmail}");
                    }
                    else
                    {
                        logger.LogInformation($"User chủ shop {shopOwnerEmail} đã tồn tại.");
                    }

                    // --- 2. Tạo Shop (Nếu chưa có) ---
                    var shopName = "Tiệm Xôi Chè Cô Bốn";
                    var shop = await context.Shops.FirstOrDefaultAsync(s => s.Name == shopName);
                    if (shop == null)
                    {
                        shop = new Shop
                        {
                            Name = shopName,
                            OwnerUserId = shopOwner.Id, // Gán User vừa tạo/tìm thấy
                            SearchableName = StringUtils.RemoveAccents(shopName)
                            // Bạn có thể thêm Description, ImageUrl cho shop ở đây
                        };
                        await context.Shops.AddAsync(shop);
                        await context.SaveChangesAsync(); // Lưu shop để lấy ShopId

                        // Cập nhật lại user với ShopId
                        shopOwner.ShopId = shop.Id;
                        context.AppUsers.Update(shopOwner); // Đánh dấu là user đã được cập nhật
                        await context.SaveChangesAsync(); // Lưu lại user

                        logger.LogInformation($"Đã tạo shop: {shopName}");
                    }
                    else
                    {
                        logger.LogInformation($"Shop {shopName} đã tồn tại.");
                        // Đảm bảo User được liên kết đúng
                        if (shopOwner.ShopId == null || shopOwner.ShopId != shop.Id)
                        {
                            shopOwner.ShopId = shop.Id;
                            if (!shopOwner.Roles.Contains("Shop")) shopOwner.Roles.Add("Shop");
                            context.AppUsers.Update(shopOwner);
                            await context.SaveChangesAsync();
                        }
                    }

                    // --- 3. Tạo Categories (Nếu chưa có) ---
                    var xoiCheCategoryName = "Xôi - Chè";
                    var comboCategoryName = "Combo Tiết Kiệm";

                    var xoiCheCategory = await context.ProductCategories.FirstOrDefaultAsync(c => c.Name == xoiCheCategoryName);
                    if (xoiCheCategory == null)
                    {
                        xoiCheCategory = new ProductCategory { Name = xoiCheCategoryName };
                        await context.ProductCategories.AddAsync(xoiCheCategory);
                        logger.LogInformation($"Đã tạo category: {xoiCheCategoryName}");
                    }

                    var comboCategory = await context.ProductCategories.FirstOrDefaultAsync(c => c.Name == comboCategoryName);
                    if (comboCategory == null)
                    {
                        comboCategory = new ProductCategory { Name = comboCategoryName };
                        await context.ProductCategories.AddAsync(comboCategory);
                        logger.LogInformation($"Đã tạo category: {comboCategoryName}");
                    }
                    // Lưu Categories nếu có thay đổi
                    await context.SaveChangesAsync();


                    // --- 4. Thêm Sản phẩm (Chỉ thêm nếu chưa có) ---
                    var placeholderImageUrl = "your_placeholder_image_url.jpg"; // Thay bằng link ảnh thật hoặc mặc định

                    var productsToAdd = new List<Product>
                    {
                        // ----- Xôi - Chè -----
                        new Product {
                            Name = "Xôi ngũ sắc",
                            Description = "Gồm 5 màu tự nhiên từ nguyên liệu truyền thống (gấc, lá cẩm,hoa đậu biếc, lá dứa, đậu xanh).",
                            Features = "Mang ý nghĩa phong thủy – tượng trưng cho ngũ hành cân bằng, may mắn và thịnh vượng. ; Dẻo thơm, vị ngọt thanh tự nhiên. ; Phù hợp cho các lễ cúng, khai trương, tân gia, đầy tháng.",
                            Images = new List<ProductImage>
                            {
                                new ProductImage { ImageUrl = "https://scontent.fsgn2-8.fna.fbcdn.net/v/t39.30808-6/541379662_1197433269075201_5707430402474025215_n.jpg?stp=c0.47.1299.1299a_cp6_dst-jpg_s206x206_tt6&_nc_cat=102&ccb=1-7&_nc_sid=50ad20&_nc_eui2=AeEqzViROL76davPgF53ZBogY31Co_mapvFjfUKj-Zqm8WRhMRO9zXk061XsPiZ3_15dHhCece_YWKrjDEWvXK5A&_nc_ohc=uviOn-MFL_gQ7kNvwF6hGzY&_nc_oc=AdmU5bfA5SvacY_8zOenBogssMqQBWIruFYdBplaAMu9qwnp71APxUjTZ97Zbz_Mrz4&_nc_zt=23&_nc_ht=scontent.fsgn2-8.fna&_nc_gid=yZJ0UNwiKf8lAvK1WuKqNw&oh=00_AfewNq-l4kqT4-Mw5cnS-v1NObdjynt7e2K2AmHQIRvO3Q&oe=6908AA74" },
                            },
                            BasePrice = 100000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:",  "Việt Nam" },
                                { "Bảo quản:", "Nơi khô ráo, thoáng mát." },
                                { "Hạn Sử Dụng:", "1-2 ngày" },
                                { "Trọng lượng:", "800g-1kg."}
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                            },
                        new Product {
                            Name = "Rau câu hoa sen",
                            Description = "Trang trí tinh tế với hình hoa sen – biểu tượng của sự thanh khiết, bình an.",
                            Features = "Làm từ thạch rau câu tự nhiên, vị ngọt nhẹ và mát lạnh. ; Phù hợp cho lễ cúng, tiệc chay, hoặc làm quà tặng ý nghĩa.",
                            Images = new List<ProductImage>
                            {
                                new ProductImage  { ImageUrl = "https://scontent.fsgn2-7.fna.fbcdn.net/v/t39.30808-6/487240548_1075089671309562_21478582763857565_n.jpg?_nc_cat=108&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeHmwrMnDxh4TNuPg9s9hdVITSZ3oifAXvFNJneiJ8Be8bqSFh8dOk0D8I5rCcleWBCWG5dgBTRbb0jwULhTvIiP&_nc_ohc=RRq_s9Bq_YoQ7kNvwHgc7Ce&_nc_oc=Admt5yavZ82673PEy7Ndrv-52N32H5ckvfdBl_quoebqUB5ZtRsy7pPl9Lhi1IWOYn4&_nc_zt=23&_nc_ht=scontent.fsgn2-7.fna&_nc_gid=xUPpO3Kq1ZWyQYhYj42x3Q&oh=00_AfeMvmityO0Gmbg6-v1_LZuK51ioSvTTC9XATqUHAWCwqQ&oe=6908CB9F" }
                            },
                            BasePrice = 35000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                {"Bảo quản:", "ngăn mát tủ lạnh 3-5 độ C." },
                                {"HSD:", "1-2 ngày" },
                                {"Trọng lượng:", "300g" }
                            }),
                                    StockQuantity=100,
                                    ProductCategoryId = xoiCheCategory.Id,
                                    ShopId = shop.Id,
                            },
                        new Product {
                            Name = "Xôi lá cẩm nhân đậu xanh",
                            Description = "Màu tím tự nhiên từ lá cẩm, tượng trưng cho sự sung túc và viên mãn.",
                            Features = "Nhân đậu xanh mịn màng, vị ngọt béo bùi. ; Dẻo thơm, đóng gói tiện lợi cho cúng lễ hoặc tiệc nhỏ.",
                            Images = new List<ProductImage>{
                                new ProductImage { ImageUrl = "https://scontent.fsgn2-7.fna.fbcdn.net/v/t39.30808-6/496227044_1106968004788395_3014107575339475341_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=108&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeGVgjWVYaYLZIy1b-N9cadhq8kfs4DFzsmryR-zgMXOyasUoSXEf-pei37abwRC0N8mEolnnPRXtualswPvqZmL&_nc_ohc=-xy4Phq3F4UQ7kNvwFgvtI3&_nc_oc=AdkPp7AQPlmwttn8oDI4HFbc-7b97OFGLI5ioggBFlew9QhTSkEuvPA8Tc1uAfRpid4&_nc_zt=23&_nc_ht=scontent.fsgn2-7.fna&_nc_gid=hGTw_-E82x8KLfdAAmTxIg&oh=00_AffQv6aAIuxXeoKJwy8J4uQ4XttRP3nSShsc2F1gh1rCHg&oe=69089E9E" }
                            },
                            BasePrice = 100000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát." },
                                { "HSD:", "1-2 ngày" },
                                { "Trọng lượng:", "300g - 800g" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Rau câu dừa plan",
                            Description = "Kết hợp vị béo của dừa và lớp flan mềm mịn.",
                            Features = "Mát lạnh, ngọt dịu, dùng tốt cho mùa hè hoặc lễ cúng chay.; Đóng hũ nhỏ gọn, dễ bảo quản.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-8.fna.fbcdn.net/v/t39.30808-6/555565502_1220384953446699_3145249367105095901_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=102&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeHkjVhWT5pU8vwbwFj-Q08WFfqqLJzB0RkV-qosnMHRGZWkHjd4gpMhgvHLc3x-nsMZYU_ghirUakOdrsLWMzFA&_nc_ohc=Il1mv_nIUxwQ7kNvwF6nW3l&_nc_oc=AdlKnd-EPqyVJf0ZmnKpfKPh8sI-lgwVZqPqHQ00YmyxlA6Ahwsw0pUn_i2Ie8tOlQM&_nc_zt=23&_nc_ht=scontent.fsgn2-8.fna&_nc_gid=vmsxcgP5bZbtDqPdMuVl-Q&oh=00_AfdMLhslzizcrq6XihPiN46IIe9RqLmsFaF0e7tZ_Eohpg&oe=69089F39" }
                            },
                            BasePrice = 8000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "ngăn mát tủ lạnh 3-5 độ C" },
                                { "HSD:", "1-2 ngày"},
                                {"Trọng lượng:", "300g - 800g" }
                            }),
                        StockQuantity=100,
                        ProductCategoryId = xoiCheCategory.Id,
                        ShopId = shop.Id},
                        new Product {
                            Name = "Set xôi gấc mặt sên đậu",
                            Description = "Xôi gấc đỏ tượng trưng cho may mắn, phúc lộc.",
                            Features = "Mặt sên đậu được trang trí tinh tế, thể hiện sự sung túc và đủ đầy. ; Set 13 hộp nhỏ tiện dụng cho mâm cúng đầy tháng, thôi nôi.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/485405049_1065738365578026_1796382996166533552_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=111&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFID6ZCd48Gkz5j6MbeMDrwltcYxIM-OM6W1xjEgz44znVOkLQUYCDhtNdyi_sldNMYNTzulWJfcYmSoe818DUT&_nc_ohc=ZJpTmYvqglsQ7kNvwEb7H_e&_nc_oc=Adn6rhK6H7JXBSe71Y3quqBOFKM-IxWEmizXTppZUW_78r5jc4cA58Hc9rcVyVL5gcY&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=c3ifc1hYwchlUbv_virz9w&oh=00_AffmYMR7R2xq4zwo_A_S2yFvB-xYRLiWLG2ZTjL7XhvGbA&oe=6908A027" }
                            },
                            BasePrice = 430000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo,thoáng mát" },
                                { "HSD:", "1-2 ngày" },
                                {"Trọng lượng:", "12 hộp nhỏ 300g, 1 hộp lớn 500g" }
                                }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id},
                        new Product {
                            Name = "Xôi song hỷ",
                            Description = "Dành riêng cho lễ cưới hỏi – biểu tượng cho hạnh phúc đôi lứa.",
                            Features = "Màu đỏ tươi từ gấc kết hợp đậu xanh, vị ngọt thơm, mềm dẻo. ; Trang trí chữ “Song Hỷ” tinh xảo.",
                            Images = new List<ProductImage>{
                                new ProductImage { ImageUrl = "https://scontent.fsgn2-3.fna.fbcdn.net/v/t39.30808-6/481253183_1053380573480472_5949951960261771440_n.jpg?_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFRuT4nT3ve6GhZba1MYnhV-eWDm7C9s5b55YObsL2zloYIJVpkQTM5OYO4WjwcWdKmVzBSN8yF2umTHzjL_IMn&_nc_ohc=c8BiPlGyaJwQ7kNvwEed3MB&_nc_oc=AdlKYyPWU0HRQat3h2FB-rgSCwOtfN7jI_aJwxE_zr3hkYbUV3NIGIIvxLnKNpJ8C7I&_nc_zt=23&_nc_ht=scontent.fsgn2-3.fna&_nc_gid=vsUxzv-eDeyHQ1agjqJ--g&oh=00_Afcb7M8Jm-n33QdK-0RcoRDSwIKgCIYPHmzVjgELrF3v2A&oe=6908B1D6" }
                            },
                            BasePrice = 100000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1-2 ngày" },
                                {"Trọng lượng:", "500g - 600g"}
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id},
                        new Product {
                            Name = "Set chè trôi nước",
                            Description = "Viên chè tròn tượng trưng cho sự viên mãn, đoàn tụ.",
                            Features = "Nhân đậu xanh ngọt béo, nước gừng ấm lòng. ; Phù hợp cho lễ cúng ông bà, lễ đầy tháng, thôi nôi.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-5.fna.fbcdn.net/v/t39.30808-6/483676793_1060890702729459_540921206818477320_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=104&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeEiBysK0rAlzml3FPx-RzSHqP2_lA26qPuo_b-UDbqo-8k8AbfZjF1SqJ4UafH6pvIxIakesOGITv_DtzV0w_AV&_nc_ohc=TomRwVuSj4sQ7kNvwFnLsr-&_nc_oc=AdmRy_czG7Tyr8xjxPZX56DWt5Gw5_A8QasMKoesWeK1YSC02_w4Swnv5gptG6oqF-0&_nc_zt=23&_nc_ht=scontent.fsgn2-5.fna&_nc_gid=ic-FUqFjsh7L8rBvepFwGQ&oh=00_Afd-timBC1_T-NL2NB65SEd8JnOSOZnLfYOd5FWRSoaMIg&oe=6908C421" },
                                new ProductImage { ImageUrl = "https://scontent.fdad3-1.fna.fbcdn.net/v/t39.30808-6/488710767_1079528010865728_1544369605902230390_n.jpg?_nc_cat=108&ccb=1-7&_nc_sid=127cfc&_nc_eui2=AeGpRnSvCUwGhthy763OHG1M_qnOOGnSRRn-qc44adJFGdJRWW_nqk1DCImpypDrtLkJUxBF_r26QJ7cYi_0ChcX&_nc_ohc=i-FkDoBQgcAQ7kNvwG3RPVD&_nc_oc=AdkbU-P6Uk5zvcDhwfqSP62gSEinae9Tgl4hSseArA4aGMwD9KfalMe6r9yfnWb-8To&_nc_zt=23&_nc_ht=scontent.fdad3-1.fna&_nc_gid=Knr3uYYc2hfadHFMsz0wOw&oh=00_Afe3g_exF8YO9Ia9jrjPGZsGVc2OUoqFsp1eDIgfatv1tQ&oe=690ACC95" },
                                new ProductImage { ImageUrl = "https://scontent.fdad3-5.fna.fbcdn.net/v/t39.30808-6/485768659_1069212518563944_2335930251528614192_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFA-U4NKJyQakrbnGYtWAogBV6s0QO2_jEFXqzRA7b-MXebdcWXsjuI87g4NyY1WQBb-anUwx_rR8MdTVSsaeXU&_nc_ohc=cHhhfO22Ie8Q7kNvwEnsJhg&_nc_oc=AdlgogA27D-70GPOXNFiXJtkqRyvd7rV1XyZCtq9grr1v31C1854FGuTwz1w-mKMpJE&_nc_zt=23&_nc_ht=scontent.fdad3-5.fna&_nc_gid=UyyUjwb2mKy2pK134Pd5PQ&oh=00_AfePunaBJW3pOvBL7NM7_wa79XKCXtnUJG5k9SlneyyrJA&oe=690AB688" }
                            },
                            BasePrice = 250000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                            { "Xuất xứ:", "Việt Nam" },
                            { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                            { "HSD:", "1-2 ngày" },
                            {"Trọng lượng:", "13 hộp nhỏ 500g" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Set xôi gấc đơn sắc",
                            Description = "Màu đỏ gấc tự nhiên, biểu trưng cho may mắn và thịnh vượng.",
                            Features = "Dẻo mềm, hương vị truyền thống, phù hợp lễ khai trương – cúng đầu năm.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-3.fna.fbcdn.net/v/t39.30808-6/469533049_1085514669730491_5139790982711780290_n.jpg?_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeEFhT5O-B0WEZY7ox-UpLA4CbfaankLjN8Jt9pqeQuM32bSVPyLEpXWMkPyV3HSjbsx0pNewQqTaDdWEdMzPC0Q&_nc_ohc=fJGRLfJoiYkQ7kNvwHd1GqO&_nc_oc=AdmrlpYNDI9oWJHTJRHEdskUi5W_TTEYVXui7o8uhqEasE-r6PcOIbxJ0L25swYpBII&_nc_zt=23&_nc_ht=scontent.fsgn2-3.fna&_nc_gid=G371dBWwwwDMwk5UNTb7rw&oh=00_AfdzgXDduWXmpV1yAdovMRlSQDewnvZkO8uYLxccUj3kdQ&oe=690899DA" }
                            },
                            BasePrice = 390000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam"},
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1-2 ngày" },
                                { "Trọng lượng:", "12 hộp nhỏ 300g, 1 hộp lớn 500g" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Mẹt thần tài thịt luộc",
                            Description = "Gồm đầy đủ món cúng truyền thống: thịt luộc, tôm, trứng, xôi, chè,bánh hỏi.",
                            Features = "Được bày trí trên mẹt tre truyền thống, trang trọng và tiện lợi. ; Thích hợp cho cúng Thần Tài – cầu tài lộc, bình an.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-3.fna.fbcdn.net/v/t39.30808-6/556875537_1220384980113363_8758857578471060149_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeHo2AS1nnhbAAUiZd1z7rox7QkoIwgcrL7tCSgjCBysvshurFzZE4s3QnD8ZFFAJ1Nlv8PIfDVK6bAbRov7mLgW&_nc_ohc=Z1JdRHvKQJ8Q7kNvwFaFOKH&_nc_oc=AdlQ1bPiX57P84Kuz_S_JuPIn-XEpaVOvOyAZyVWokLpzlC7Wqa7PUeoNjDbEz6z0JA&_nc_zt=23&_nc_ht=scontent.fsgn2-3.fna&_nc_gid=q4u_PjYFKy11S69qYQIDZg&oh=00_AfcO91ic5LQXn8msZrTcNGc3gxPZWIdDVkuKtGWo-epruQ&oe=6908C7E8" }
                            },
                            BasePrice = 300000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam"},
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1 ngày"}
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Mẹt tam sên thần tài heo quay",
                            Description = "Gồm heo quay, trứng, tôm, xôi, chè.",
                            Features = "Heo quay giòn da, thơm ngon – tượng trưng cho sung túc và phát đạt. ; Dành cho lễ cúng Thần Tài – Thổ Địa.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-5.fna.fbcdn.net/v/t39.30808-6/558446411_1220384973446697_3422695995249162276_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=104&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeG6zM0FTS4RWhdgAmDavwBoE2HVD-61bfcTYdUP7rVt9_7lcPqSXXsVZlRczmmFnICopfa64XbZXBiRRlDwrdhW&_nc_ohc=dGnZDi0tG4wQ7kNvwExFKO5&_nc_oc=AdngaHbMq81G6PlCvSaemMhUaZsKieT7MQL1fW2y09-bp_t_4Z9yQxgEcm1112i1j5Q&_nc_zt=23&_nc_ht=scontent.fsgn2-5.fna&_nc_gid=fOO8LZ4HvVhIAYEKXnIZfg&oh=00_Afe6PFbuSZiA_78MDUgbaZrJUA1kxe-pjaX3fjeNYTG9oA&oe=6908A556" }
                            },
                            BasePrice = 350000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1 ngày." }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Xôi chim cút",
                            Description = "Hương vị đặc trưng, dẻo thơm, bổ dưỡng.",
                            Features = "Chim cút tượng trưng cho phúc lộc và sức khỏe. ; Dùng trong lễ cúng khai trương, thần tài, Tết cổ truyền.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-9.fna.fbcdn.net/v/t39.30808-6/486314003_1071239368361259_1454308254779610938_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=103&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeGghPf1bk8uyUe9sDXx-FJm_VbKKG16ASX9VsoobXoBJQ9Ao_WgXnN65h1BuSkD-vy6SGHkwkE9p_DJlwJAXJnQ&_nc_ohc=pH_F6mQuCCUQ7kNvwECVa-m&_nc_oc=AdkP1IwRbhjuXKlXRqUxerc24hkWOvbfvtCMTHhk9E59FwpT5H6m5uPySl1_NzospGo&_nc_zt=23&_nc_ht=scontent.fsgn2-9.fna&_nc_gid=qmBEiHh9UuuWJtdj4WhD-g&oh=00_AfeJkpLivr-gS4RG69_VgvEe4c0lDGgAf5QYd21_YzX41g&oe=6908CB02" }
                            },
                            BasePrice = 110000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1 ngày." }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Rau câu cá chép",
                            Description = "Hình cá chép – biểu tượng cho thành công, thăng tiến.",
                            Features = "Vị ngọt thanh, màu sắc tươi sáng, phù hợp cúng lễ đầu năm – thi cử.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-4.fna.fbcdn.net/v/t39.30808-6/485843465_1068761881942341_6708689625131368850_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=101&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeH810Qyef4ueZ0YE0mSfxkyu2G-F5ksUR67Yb4XmSxRHmruKk1rdhms2Tp76WUixQWRvTPj7ZqkZok1jieZXVEI&_nc_ohc=DN29CAp2tcUQ7kNvwEKDaxg&_nc_oc=AdmE2vWTl2EpEKcGBSahB1MPqs3h0l5o-VjI20yOPfPpUg9gJWBKxRv2CKlpXxlJI_c&_nc_zt=23&_nc_ht=scontent.fsgn2-4.fna&_nc_gid=JY9_W5TQdifpgMARpTqX1w&oh=00_AffuhldcbewJK9ra14_XLTlyrb53_vJvOaii52tdkyCCPQ&oe=6908BC2A" }
                            },
                            BasePrice = 80000,
                            Specifications = "Xuất xứ: Việt Nam ; Bảo quản: ngăn mát tủ lạnh 3-5 độ C ; HSD: 1-2 ngày",
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Xôi gấc hoa lá sen",
                            Description = "Tạo hình hoa sen tinh tế, ý nghĩa thanh khiết – phúc lành.",
                            Features = "Màu đỏ gấc rực rỡ, tượng trưng cho hạnh phúc và tài lộc.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-3.fna.fbcdn.net/v/t39.30808-6/485148561_1069212295230633_2437587401656087306_n.jpg?_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFVA_PX3bqsbvicicRKyxpdihDsLH_mHHuKEOwsf-Yce_8nJPx27wGVDzW_h5s_ZZoCJrRDmejHmWVq_lQzN-gG&_nc_ohc=RvpLKnro1VcQ7kNvwFKzHRI&_nc_oc=Adn-DYjrDB8IcaNdHnOe3fEkmzjQt4fTg5K_MVdtXJh_uvbIsYEY_j7rhLmGz8OTpRo&_nc_zt=23&_nc_ht=scontent.fsgn2-3.fna&_nc_gid=_nNEfO3w8HVj90vIct_vyg&oh=00_AfcZjpzAe8XuD4yzfqogb6u7SzggmPvm3T3H8GDNOuCAuw&oe=6908B34A" }
                            },
                            BasePrice = 35000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1-2 ngày" },
                                { "Trọng lượng:", "300g" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Rau câu chữ Lộc",
                            Description = "Tạo hình chữ “Lộc” – cầu may mắn, tài lộc và thịnh vượng.",
                            Features = "Màu sắc đẹp mắt, vị ngọt thanh mát. ; Phù hợp cúng Tết, khai trương, lễ đầu năm.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-3.fna.fbcdn.net/v/t39.30808-6/487200728_1075092294642633_5422837502837129113_n.jpg?_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeHUoC62YwpV0TyQ1ZJwfjnSZ5nh3mhDWCNnmeHeaENYIzjuTRuL1lG2zplBA2F2jGtXr4UXryk8gOx1ENS07-Qk&_nc_ohc=N6BNRdnTEqEQ7kNvwF923Th&_nc_oc=Adk8V29Sf5JgdCvmhycbdFQ43O2JNXTE5knUcW6q6M49li08H6u4HiONGfv-kOL7AqA&_nc_zt=23&_nc_ht=scontent.fsgn2-3.fna&_nc_gid=DdM47XLaG4pIk7lEujs89A&oh=00_AffN77xiCe-22Ypjw1wniP62JpEUVT_01jqyuNljbxHNhQ&oe=6908C0AC" }
                            },
                            BasePrice = 65000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                {"Xuất xứ:", "Việt Nam" },
                                {"Bảo quản:", "ngăn mát tủ lạnh 3-5 độ C" },
                                {"HSD:", "1-2 ngày" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Chè cốm khoai môn",
                            Description = "Sự kết hợp hài hòa giữa cốm xanh thơm và khoai môn bùi béo.",
                            Features = "Vị ngọt thanh, mang đậm hương vị Bắc Bộ truyền thống.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/487568511_1075089154642947_2033796478254800037_n.jpg?_nc_cat=110&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFD5Fk92JaF_eYTYzIokvlM1kzgiZmYJdnWTOCJmZgl2V1WlcSlgkEOE3H24guLj1o6lc7o0FFpQ7eIRBZ06Tdm&_nc_ohc=8kstRYU6uFcQ7kNvwG1X1l7&_nc_oc=AdkbYZNUZBM6o2GN9QF2CTw7G1SrFNpc6LH9yt-LkfZECFJy2FTQc0Lk7UhTs9D8dyA&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=LEqbrA9raIfYGmMeHNlTXA&oh=00_AfdRKsvy-c9SNOZBVqv4Zi75cDOkuT0MGkJ4vhGi58RudQ&oe=69089EF1" }
                            },
                            BasePrice = 100000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                {"Xuất xứ:", "Việt Nam" },
                                {"Bảo quản:", "nơi khô ráo, thoáng mát" },
                                {"HSD:", "1-2 ngày" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Heo quay con 4-4,5kg",
                            Description = "Heo quay da giòn, thịt mềm thơm, màu vàng óng đẹp mắt.",
                            Features = "Là lễ vật quan trọng trong các lễ cúng lớn: khai trương, tân gia, cúng Thần Tài.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-9.fna.fbcdn.net/v/t39.30808-6/484347151_1060974182721111_56654645468955225_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=103&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeH4QNseUQr2O0Xv6gnad3fOQ-HJIL5aBjlD4ckgvloGOeF-SfmXN-ukKiE8SZhL6EOi2YMku8Zxq8mixxJxppqF&_nc_ohc=5z_lRtFOAlMQ7kNvwGxY193&_nc_oc=AdlFjIFQcMGnKleUVoBEB757rBpUFPJsrFP0dFla1y7afFh2J5V6ui5ZbD0mL-q2viw&_nc_zt=23&_nc_ht=scontent.fsgn2-9.fna&_nc_gid=sNWA4I_SVy19weXT-mCQ7Q&oh=00_AfdkjjV9hW0B8E2-EdpplWn5EPqn_oQnd4vJIDpWs-Uf5Q&oe=6908AD96" }
                            },
                            BasePrice = 4000000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1-2 ngày" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id},

                        // ----- Combo Tiết Kiệm -----
                        new Product {
                            Name = "Mâm cúng Đầy tháng/ Thôi nôi trọn gói (Gói 1)",
                            Description = "Đầy đủ lễ vật: xôi, chè, rau câu, trái cây, gà luộc, giấy cúng, hoa.",
                            Features = "Chuẩn bị trọn gói, sạch sẽ, gọn gàng, đảm bảo đúng nghi lễ truyền thống.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-10.fna.fbcdn.net/v/t39.30808-6/487957038_2518473248495827_7445081025542901339_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=109&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFI4v2KgV3LDeHYJ9RjSNuY9ioQTuHHqDz2KhBO4ceoPOItiKspNSnlbSwAHr0ltv_nhFWBDt3jKpbeGUjxRvOo&_nc_ohc=bLRqx-T5lIoQ7kNvwFGilH6&_nc_oc=Adlb-VPu4Hgky-BO2iLCUsDlUEpppkfEV6BqOk8vf3B7S1cpiqaB-WEVQzqpbJZ-9qE&_nc_zt=23&_nc_ht=scontent.fsgn2-10.fna&_nc_gid=UGC5NC-OQ8KNRrIMsgO7Hg&oh=00_Afc3Rb0HlgrdhJTMlanZX4L3TOd3lTxc7YjRLAi7LCcrSQ&oe=6908AAD8" }
                            },
                            BasePrice = 1700000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1-2 ngày" },
                                { "Bao gồm:", "13 hộp xôi, 13 chén chè, 13 dĩa rau câu, 1 dĩa trái cây ngũ quả, 1 con gà luộc, giấy cúng, hoa" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = comboCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Mâm cúng Đầy tháng/ Thôi nôi trọn gói (Gói 2)",
                            Description = "Đầy đủ lễ vật: xôi, chè, rau câu, trái cây, gà luộc, giấy cúng, hoa.",
                            Features = "Chuẩn bị trọn gói, sạch sẽ, gọn gàng, đảm bảo đúng nghi lễ truyền thống.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/484903604_1063987839086412_6313309244166366364_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=110&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFyD2mwg2IaPil7cA6i3Hh3IaUr41IwGY0hpSvjUjAZjam--U6hsMS8Px1dsowzpOJaa7U2tr6Jd-7R1e7AWrAs&_nc_ohc=5opu-WIIRZMQ7kNvwHtdc42&_nc_oc=AdkkbrHjKtnNIpW9fvjDrfsFeOaR4Dja4PqJ-deDmGjoc2y8-9jKOTslQDveF3F4tGk&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=M-zRJZK1GUGK81pZaKu7wg&oh=00_AfeYJ4UScMymvNJxFNWbol4qN3iTAEdOk_rr8OFq1XnoRg&oe=6908BE21" }
                            },
                            BasePrice = 2200000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1-2 ngày" },
                                { "Bao gồm:", "13 dĩa xôi, 13 dĩa rau câu, 13 dĩa chè, 1 dĩa trái cây ngũ quả, 1 con gà luộc, giấy cúng, hoa" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = comboCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Mâm cúng Đầy tháng/ Thôi nôi trọn gói (Gói 3)",
                            Description = "Đầy đủ lễ vật: xôi, chè, rau câu, trái cây, gà luộc, giấy cúng, hoa.",
                            Features = "Chuẩn bị trọn gói, sạch sẽ, gọn gàng, đảm bảo đúng nghi lễ truyền thống.",
                            Images = new List<ProductImage>{
                                new ProductImage {ImageUrl = "https://scontent.fsgn2-3.fna.fbcdn.net/v/t39.30808-6/484878812_1063770679108128_5080873628317308610_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFPBml6yQxZCPYJTNOjiMh3s95r2fISJgCz3mvZ8hImAJXrlkWtiR1c15TzAzEKZ7LiLkUsJmcL66Ft70-VtR74&_nc_ohc=D7GcBQh4jk8Q7kNvwEfnKpr&_nc_oc=AdnNJxuGSH0LydkXvRMR9mii0nab-szSK-AhhQt2JHWu_ovkWe4Jg4-giZjKMLH5TwE&_nc_zt=23&_nc_ht=scontent.fsgn2-3.fna&_nc_gid=YmZ3AZ90BwvEFuW8pQrIQw&oh=00_AfcaF1GUZYYxz4CJT4kaQ7AaSY0z0RZi1OFuK7A5q0DU1g&oe=6908CEB9" }
                            },
                            BasePrice = 2800000,
                            Specifications = JsonSerializer.Serialize(new Dictionary<string, string>
                            {
                                { "Xuất xứ:", "Việt Nam" },
                                { "Bảo quản:", "nơi khô ráo, thoáng mát" },
                                { "HSD:", "1-2 ngày" },
                                { "Bao gồm:", "13 dĩa xôi, 13 dĩa rau câu, 13 bánh bao, 13 chai sữa chua thạch, 1 giỏ trái cây, 1 con gà luộc, giấy cúng, hoa" }
                            }),
                            StockQuantity=100,
                            ProductCategoryId = comboCategory.Id,
                            ShopId = shop.Id
                        },
                    };

                    int addedCount = 0;
                    foreach (var product in productsToAdd)
                    {
                        // Kiểm tra xem sản phẩm đã tồn tại chưa (theo Tên + ShopId)
                        if (!await context.Products.AnyAsync(p => p.Name == product.Name && p.ShopId == shop.Id))
                        {
                            // Tự động tạo SearchableName
                            product.SearchableName = StringUtils.RemoveAccents(product.Name);
                            await context.Products.AddAsync(product);
                            addedCount++;
                        }
                    }

                    if (addedCount > 0)
                    {
                        await context.SaveChangesAsync();
                        logger.LogInformation($"Đã thêm {addedCount} sản phẩm mới cho shop {shopName}.");
                    }
                    else
                    {
                        logger.LogInformation($"Không có sản phẩm mới nào được thêm cho shop {shopName}.");
                    }

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Đã xảy ra lỗi khi seeding dữ liệu Shop và Sản phẩm.");
                }
            }
        }
    }
}
