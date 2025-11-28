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
        private static async Task<byte[]> DownloadImageAsync(string url)
        {
            try
            {
                using var client = new HttpClient();
                return await client.GetByteArrayAsync(url);
            }
            catch
            {
                // Nếu link chết hoặc lỗi mạng, trả về mảng rỗng để không crash app
                return Array.Empty<byte>();
            }
        }

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
                        shopOwner.Shop = shop;
                        context.AppUsers.Update(shopOwner); // Đánh dấu là user đã được cập nhật
                        await context.SaveChangesAsync(); // Lưu lại user

                        logger.LogInformation($"Đã tạo shop: {shopName}");
                    }
                    else
                    {
                        logger.LogInformation($"Shop {shopName} đã tồn tại.");
                        // Đảm bảo User được liên kết đúng
                        if (shopOwner.Shop == null || shopOwner.Shop.Id != shop.Id)
                        {
                            shopOwner.Shop = shop;
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-7.fna.fbcdn.net/v/t39.30808-6/541025231_1197433199075208_6392725115706814321_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=100&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeGzI1pm1fRE4qJLdVgbo0DTnHjFuHDp3z2ceMW4cOnfPS3Gu3oHlMtWe53cPonq7tyEq3OAYR-RRb-y1OchhS5W&_nc_ohc=2gwuwaJE0RQQ7kNvwFHfa7k&_nc_oc=AdmwaZpDUJeV4v2pJiVqPPsJshfmjnzrsX_URsoS6-snIEByO0IZ75SepjBcw1ZUhbmSo_QGOXHGp23A7GdXVwdr&_nc_zt=23&_nc_ht=scontent.fsgn2-7.fna&_nc_gid=sPRQFMZjuaQiIKuPLtiSNg&oh=00_Afg5ouCq2E-f5oJz-QySKDSCUvRCF0uFif0ex2yS7CCV9Q&oe=692E3212"),
                                    ImageMimeType = "image/jpeg"
                                },
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
                                new ProductImage  { 
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-9.fna.fbcdn.net/v/t39.30808-6/488590584_1079531694198693_8674482424017446483_n.jpg?_nc_cat=106&ccb=1-7&_nc_sid=127cfc&_nc_eui2=AeFaiU6Q56yBiyBHPmZEoFHcZbXv0xu21tplte_TG7bW2nZESoYmUb993tmvooyqUalE1d-IU03ftLSqNFFPeL0C&_nc_ohc=eaBrOOQRQyUQ7kNvwENx88w&_nc_oc=AdlEWKQgbIV5gaIuZf0UUzifNT7H2awZg_nB6ewx33khBlu9Y7u1-uySV49U3LV78h7Mmlu99Q7L-z_jeT1Yz-WJ&_nc_zt=23&_nc_ht=scontent.fsgn2-9.fna&_nc_gid=8ohz8-Q-kFDtS69wuXzS3w&oh=00_AfgRvxXP21YeZkx2Eg3PZKbztST8cXKih6I_dv8p4qHw7A&oe=692E16B2"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-7.fna.fbcdn.net/v/t39.30808-6/558740521_1230062299145631_7503797338123846275_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=100&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeF7J3v5K_IPKqSw-ze-gtVtginWs9oWmkSCKdaz2haaRP7sfuDQi0VvrRCDoxx1MxgApiiOY7F5DO_tJ0rJ7xxj&_nc_ohc=7uOk6B7Rg34Q7kNvwHc986D&_nc_oc=AdmE_nfQ5HjhPIfRadHGtKacxWfmxOzAe0sCitsF3dQiOXvfKeMJgpAo70rz9AmVQEE9H3DXG_IjixfX6VpTVgRc&_nc_zt=23&_nc_ht=scontent.fsgn2-7.fna&_nc_gid=4jylqKTeyCOfQs1OE_BPlA&oh=00_AfjGwTvBe1qBihktAwblEuJ4ZA90BjJNr6G8xBOgwl4CIw&oe=692E243B"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-8.fna.fbcdn.net/v/t39.30808-6/555565502_1220384953446699_3145249367105095901_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=102&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeHkjVhWT5pU8vwbwFj-Q08WFfqqLJzB0RkV-qosnMHRGZWkHjd4gpMhgvHLc3x-nsMZYU_ghirUakOdrsLWMzFA&_nc_ohc=7BHzlIVdrSQQ7kNvwEumQe0&_nc_oc=AdnlVgsCn0lrI7goYU3iSWvYIUuKiNSdgjzQfzsk11F1XbOrwe6IfbhVgn-csZHBO4u6YW_pzMeLq4OgE-Dl6I9V&_nc_zt=23&_nc_ht=scontent.fsgn2-8.fna&_nc_gid=Mvf188G6yYOcARzp09kISQ&oh=00_AfioXBbu_LtqRE6B8bNuBWiENSry8ckRTG5ZxBEcPz6E3g&oe=692E31F9"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-8.fna.fbcdn.net/v/t39.30808-6/558111759_1220384900113371_5687351138875825458_n.jpg?_nc_cat=102&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeEAEps398XbwkgiBr9GxDbp5XtZxl2dRgPle1nGXZ1GA1xK1m1Sw-VqA58hnXCuycLb7_kH4or14s8O8S5qhopv&_nc_ohc=PVCC8pEDoW0Q7kNvwGeTodS&_nc_oc=AdkIYdjdTxTpJ3i3HQCntse0_N6Loz8ckZSCLn653s13Bvy4WQKRdPcHpvi_LnYCSg9QdksSzpCB4gePkpWRqu2L&_nc_zt=23&_nc_ht=scontent.fsgn2-8.fna&_nc_gid=iQ_mY7ccOUh-dVXKV06vdg&oh=00_AfjAIioDO5H4Cy9bw7vXcoEGzFHUwPcjjSPfIKn3NgoxNA&oe=692E2CC7"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-3.fna.fbcdn.net/v/t39.30808-6/481253183_1053380573480472_5949951960261771440_n.jpg?_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFRuT4nT3ve6GhZba1MYnhV-eWDm7C9s5b55YObsL2zloYIJVpkQTM5OYO4WjwcWdKmVzBSN8yF2umTHzjL_IMn&_nc_ohc=hA0dhfrvrDgQ7kNvwEb5Xiv&_nc_oc=Adk0h-GW6sMvOlvRlFm3GmTp-L16yQeoZkk5G8EJfofXHeoVuJZbiXcIgs6GeHvFTbF18v_4AKB8Rp4WmkUlwO0E&_nc_zt=23&_nc_ht=scontent.fsgn2-3.fna&_nc_gid=bhBs8r2CjihY99tQEv6Afw&oh=00_AfjwmCGRwqmnI9JGMuA6wG2udnHpfOfCbcVZ6dVKD4AeSA&oe=692E4496"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/484085675_1061780935973769_3019805590600455720_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=111&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeG7DT7C_la6V273BpPUKPJe-i14Mqb2Dkr6LXgypvYOSjeZFkR9h01LMXKBVL22E3902KsqrR8ay9CJqYE5rkv-&_nc_ohc=dLNQo-tXcN8Q7kNvwF_hBdZ&_nc_oc=AdnBZjMGCNN80p2zYuKIQApqSMU6ttbkgHHI-ZejOQlrsTG9J1uDmfS0ByUhr3uf1RSWUjDRxMhUniqZdLlkN5YV&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=h9LZrXlclddjKtedopL5pA&oh=00_AfinDBgNUPTa6rGPsq_dhASvbrBFvNPrR3jb4AeAsoCMKA&oe=692E2332"),
                                    ImageMimeType = "image/jpeg"
                                },
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-6.fna.fbcdn.net/v/t39.30808-6/484182317_1061781252640404_5570031133217398354_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=111&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeEqa1nuwcj1elmw8HDqmv82iCnG8zzIGtmIKcbzPMga2X7g0GnjNeUbkQaYvMXwXcVNRYu5kCi164hB7DQMoABr&_nc_ohc=maivzhzyVTAQ7kNvwFpfW-3&_nc_oc=AdktL5sNznq5hSkj0RPF1dyKk9ZpCb5bYRkikjeyAq0KV7yEDJakG9_D1TJj5GsLNzFq6TAaatdtBwr-Gu06FzmQ&_nc_zt=23&_nc_ht=scontent.fsgn2-6.fna&_nc_gid=zTxM1Gao8zaVLnMSMUYzVA&oh=00_AfhaYM6NA5nKEizeIA0ydvYe-gcGK2z0s2LWkCtNgat1aQ&oe=692E11AF"),
                                    ImageMimeType = "image/jpeg"
                                },
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-10.fna.fbcdn.net/v/t39.30808-6/485768227_1067258108759385_8302690779145725396_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=109&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeEsjejALgrMN_YFOkvJizMFEr3pk5FQpgoSvemTkVCmCr-ijlwKr8xQuvV9dtyifOWGUJvqpowy-CrBphgxV5qW&_nc_ohc=_H9_dQPtkZEQ7kNvwHmtkWf&_nc_oc=AdngfuDNLq0j30bxEbyhIBF-0nF_0i0ygXD7XfDI1uJ3Fm2iR7d6d6TLIb4_2-vvUM8VWcaXkOgrm4MY5cCFftNG&_nc_zt=23&_nc_ht=scontent.fsgn2-10.fna&_nc_gid=HtMnfGFu4jWaP79-8aIe7g&oh=00_AfiJ617z03bjM0iDAn3GrjvGGLR219cAKHzksaSyRhMFrg&oe=692E16E6"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-8.fna.fbcdn.net/v/t39.30808-6/541379662_1197433269075201_5707430402474025215_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=102&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeEqzViROL76davPgF53ZBogY31Co_mapvFjfUKj-Zqm8WRhMRO9zXk061XsPiZ3_15dHhCece_YWKrjDEWvXK5A&_nc_ohc=tvk4I_3f3wQQ7kNvwG2k3gr&_nc_oc=AdlumQaDKxT7nRlDMZlhk6A-PEqdcrbsJ7AMv0UwdqmTq2Piav3QXiGsqk3TQQkoX9j-hWpABlZO4s3JVUpv4yza&_nc_zt=23&_nc_ht=scontent.fsgn2-8.fna&_nc_gid=euXRWX8yZ5Yv6kEZoplKeA&oh=00_Afj8gZjKE9ZXvGtOlZJGU4x6Ge0T74EO1NEwi3qNfz46EQ&oe=692E3D34"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-10.fna.fbcdn.net/v/t39.30808-6/485362299_1068762915275571_106238200387156073_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=109&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeHG3mTGM6b6I92HxZa2HQ7F2ppRWpH-aXPamlFakf5pcz_w_oy8FIc4n_NetoMjYwiXxXiAbcvtTbDNHnM0hJvm&_nc_ohc=pKpyfKFOQToQ7kNvwFyZTBC&_nc_oc=AdlHI4VKYbXZDSNK8llSyn_Vddaz51igpJxspUJoY7LKaMbRw-nLgdgqgGXL8xzchPdGpeKJI9JqCWET0Y2NmANY&_nc_zt=23&_nc_ht=scontent.fsgn2-10.fna&_nc_gid=m0DO9tcmFnNexSNqpPf1sg&oh=00_AfhObtPsPvqB_iFlSuY9sH-kj1eQnfgHA5vABTX0E0Ee3g&oe=692E3F50"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-9.fna.fbcdn.net/v/t39.30808-6/484347151_1060974182721111_56654645468955225_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=103&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeH4QNseUQr2O0Xv6gnad3fOQ-HJIL5aBjlD4ckgvloGOeF-SfmXN-ukKiE8SZhL6EOi2YMku8Zxq8mixxJxppqF&_nc_ohc=NMVsJStVIl0Q7kNvwE1PaQY&_nc_oc=Adnh8086iwlquv9k9vufMEP1K1lzKZwuRbRfDsUYFdyOjJ8WAA2ZEb-kDFn9-jAWWMqzorNplZbLcxKWDgt-EuK4&_nc_zt=23&_nc_ht=scontent.fsgn2-9.fna&_nc_gid=NoEp1lGBDAxfcZbbJ072vw&oh=00_AfgtsRg-ZKhvLP_jrZlB59GbptYEfxS9-4Aez19OcEFFMQ&oe=692E4056"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-7.fna.fbcdn.net/v/t39.30808-6/486662995_1070807368404459_2726330577518962922_n.jpg?_nc_cat=108&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFtCvTtF2Y_apeX4SevHaEJ5zaAJM3LoarnNoAkzcuhqgTcPuba19gNnCMYYp_GUqvtXV7lkTh4uLt5N8P7sGUl&_nc_ohc=M5XmH_TCrBkQ7kNvwEnfB7I&_nc_oc=AdnTpM15RCe6uL2_9Eo-57ytF3GwDayQlh6UmLB4CutKT-SpEOOcj4JbqzBLXe0hv3HHcIXgEq4m-uxWtgWxrXE0&_nc_zt=23&_nc_ht=scontent.fsgn2-7.fna&_nc_gid=MABQ4sy5j3PrZBrqw6xVgw&oh=00_Afj4Hrdxh18toy0JOy3EM3mM_kGY0YM9BYiRHnkArlqdwg&oe=692E1385"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-4.fna.fbcdn.net/v/t39.30808-6/486112077_1068761968608999_3692188912100259715_n.jpg?_nc_cat=101&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeGmDm26r-bf2NNjwuDvLnsn1hyZDvcyJUvWHJkO9zIlS-f_Zd9VHzeI4K7yJIRA8JlJUx8mNYdpfnJm2Gc0nmAV&_nc_ohc=zx3aTKczRfIQ7kNvwGZO9Su&_nc_oc=Adnde1rGDkA05dmMNMfsSM_DSas1ZcP0porkPXYjUrdJVw4_i33eRd8Qsxuhs3I5cYu47cu0HCuLWzv9h7-URO0W&_nc_zt=23&_nc_ht=scontent.fsgn2-4.fna&_nc_gid=zyVOy-B87beWyz7exZ6EPQ&oh=00_AfhIzN3CNb67PRHXyr-5MnDASxbOqyBewWOeleIDYwSHRw&oe=692E1B94"),
                                    ImageMimeType = "image/jpeg"
                                }
                            },
                            BasePrice = 80000,
                            Specifications = "Xuất xứ: Việt Nam ; Bảo quản: ngăn mát tủ lạnh 3-5 độ C ; HSD: 1-2 ngày",
                            StockQuantity=100,
                            ProductCategoryId = xoiCheCategory.Id,
                            ShopId = shop.Id
                        },
                        new Product {
                            Name = "Rau câu chữ Lộc",
                            Description = "Tạo hình chữ “Lộc” – cầu may mắn, tài lộc và thịnh vượng.",
                            Features = "Màu sắc đẹp mắt, vị ngọt thanh mát. ; Phù hợp cúng Tết, khai trương, lễ đầu năm.",
                            Images = new List<ProductImage>{
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-3.fna.fbcdn.net/v/t39.30808-6/469482275_1084987819783176_8215204856689061368_n.jpg?_nc_cat=107&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFcUiPvGkL8koNI7jhm3CzQu_RMfzMQTqC79Ex_MxBOoK9xtcCkXmddBxuLVjJR4OgzyPa3VD6JTqoIoZVbF7a9&_nc_ohc=txFA51mPCWMQ7kNvwG5HFYi&_nc_oc=AdlduzYG35-j2dvOVf9eBkhbVaJ66eVD75DIGM8WyGSedDeERno8b-8X_Nwy2koyibvgHlbiBhQflwGfSg4jP0AJ&_nc_zt=23&_nc_ht=scontent.fsgn2-3.fna&_nc_gid=hfI1W2bEIufZCV3MTt8OTg&oh=00_AfjEFBfWDq2Ybt8enyhlAKQi_vZJ4zmIimWHDDapdVr8Eg&oe=692E3B18"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                            Name = "Heo quay con 4-4,5kg",
                            Description = "Heo quay da giòn, thịt mềm thơm, màu vàng óng đẹp mắt.",
                            Features = "Là lễ vật quan trọng trong các lễ cúng lớn: khai trương, tân gia, cúng Thần Tài.",
                            Images = new List<ProductImage>{
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-7.fna.fbcdn.net/v/t39.30808-6/315969821_844138713300209_7255072982214198130_n.jpg?_nc_cat=100&ccb=1-7&_nc_sid=2285d6&_nc_eui2=AeFmrz1oldvMlDWijI9fHrv0p814EHRNdYunzXgQdE11i08n9lLkXfhDjCpEZxlqxF69C_Ej7LXsYu3EuaeuCwYa&_nc_ohc=-7D6grTSx0gQ7kNvwEGQsFW&_nc_oc=Adn_GMXVC0GSgxTdBYUcwr2ugyTQ8uigaecYT8h7OgImq8egwLbqCKyr3J1XPPwaRxIYR2KPHE1RIQ-gVpE_RxMv&_nc_zt=23&_nc_ht=scontent.fsgn2-7.fna&_nc_gid=rXMic310diLGDN7O7EP8Vw&oh=00_Afh7kJTalMZU-DtC8NC37lmu7wgWXzUE1LCxSCWtaB5iIA&oe=692E2242"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-7.fna.fbcdn.net/v/t39.30808-6/546921439_1204255998392928_1577491303697229503_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=108&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeH4dFkPLMuQkwSV7v96j6iv_qPvPkBIZ7v-o-8-QEhnuywFJva0aM6Y1lF4ZX5Pit3xaB4RDGZMnXWszrQrCliq&_nc_ohc=mr5bbffmxsoQ7kNvwEo8Hxx&_nc_oc=Adk1NG5UedknhgmoLVsVNNKPLW3HulJwKeHyw2JuT7Hta-vlmiceSvl1-QE3rxWhKySemQpdaKUFompCR06wMwNE&_nc_zt=23&_nc_ht=scontent.fsgn2-7.fna&_nc_gid=6Y2r-PGcvWjCsw5B7l7oAA&oh=00_AfhQzGa63CRb0GZxaz2hVNRLLPRU7v_qmdhcM3vrGHim5g&oe=692E3605"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-8.fna.fbcdn.net/v/t39.30808-6/503979413_1125682359583626_6827542483558828841_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=102&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeHLG7zOOTxpWps397ZqnexINqynRbtS-Es2rKdFu1L4SwBR8btuIXrp7iD4oMV5Vbkz6l9r03qq_3_c88LK720L&_nc_ohc=hMi10KyOn60Q7kNvwHBzHTa&_nc_oc=Adma1TfrEumW01N21FgiDcFvUy7fVoHY9Plp0e_sy6I7TJoyJRCFmDEeh09Xk-6XZ908s67D9QUCysvOhzH4gxpP&_nc_zt=23&_nc_ht=scontent.fsgn2-8.fna&_nc_gid=QOurflERDFN9QYQdSmmaDw&oh=00_AfiCu0MsIFsb6IqWKz_q5DZl10oAiJA35KweWjrl4LTp7A&oe=692E36C8"),
                                    ImageMimeType = "image/jpeg"
                                }
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
                                new ProductImage {
                                    ImageData = await DownloadImageAsync("https://scontent.fsgn2-9.fna.fbcdn.net/v/t39.30808-6/492440413_1089481926537003_7064616119984704817_n.jpg?stp=cp6_dst-jpg_tt6&_nc_cat=106&ccb=1-7&_nc_sid=833d8c&_nc_eui2=AeFROSUWiXGkStrUCpIDRXWmw0oyTM-Z3x_DSjJMz5nfH_f88_5mAJEg4owReIwBYVMx8QfWNG74QsP-FnnZss8w&_nc_ohc=AgA3T5wLPIwQ7kNvwFKmGjk&_nc_oc=AdkZJvH41QhJY0RsmWAyaFJlryMdF9vUzomG-7bbBLmWuQuBuUysX3KaOhFiQYvcbKZePKRi9COWLFPOZI311ycF&_nc_zt=23&_nc_ht=scontent.fsgn2-9.fna&_nc_gid=lopAxPZ-QlyLLgWANzlMbA&oh=00_AfjQ8DgLpDqguuI3k2SRNd6ZOmQptUJrluzJIGZVMDLrNw&oe=692E387E"),
                                    ImageMimeType = "image/jpeg"
                                }
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
