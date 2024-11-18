using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mood.EF2;
using X.PagedList;
using System.Text.RegularExpressions;
using Mood.SanPhamViewModel;
using System.Globalization;
using System.Data.Entity;
using Microsoft.ML;
using System.IO;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;

namespace Mood.Draw
{
    public class SanphamDraw
    {
        QuanLySachDBContext db = null;
        private readonly MLContext mlContext;
        private readonly string modelPath;
        private PredictionEngine<SanphamRecomend, ProductPrediction> _predictionEngine;
        public SanphamDraw()
        {
            db = new QuanLySachDBContext();
            mlContext = new MLContext();
            modelPath = "Recomendation.zip";  // Ensure this path is correct
            LoadModel();
        }
        public SanphamDraw(QuanLySachDBContext context)
        {
            db = context;
            //Load the trained ML.NET model
            mlContext = new MLContext();
            modelPath = "Recomendation.zip";
            LoadModel();
        }
        public IEnumerable<SanPhamModel> sachcu(string searhString, int page, int pagesize)
        {
            dynamic result;
            if (!string.IsNullOrEmpty(searhString))
            {
                result = (from a in db.Categories
                          join b in db.Sanphams on a.IDCategory equals b.CategoryID
                          join c in db.NhaCungCaps on b.IDNCC equals c.IDNCC
                          join f in db.Users on b.IDNguoiTao equals f.IDUser
                          where b.Name.Contains(searhString) && b.Quanlity == "cũ nát" || b.TacGia.Contains(searhString)&& b.Quanlity == "cũ nát" || a.TenTheloai.Contains(searhString) && b.Quanlity =="cũ nát" || c.TenNCC.Contains(searhString) && b.Quanlity == "cũ nát" || f.UserName.Contains(searhString) && b.Quanlity == "cũ nát"
                          select new SanPhamModel()
                          {
                              IDContent = b.IDContent,
                              Name = b.Name,
                              Metatile = b.MetaTitle,
                              TacGia = b.TacGia,
                              NhaXuatBan = b.NhaXuatBan,
                              Soluong = b.Soluong,
                              Images = b.Images,
                              CategoryID = b.CategoryID,
                              Quanlity = b.Quanlity,
                              NgayTao = b.NgayTao,
                              NguoiTao = f.UserName,
                              Status = b.Status,
                              GiaTien = b.GiaTien,
                              TenTheloai = a.TenTheloai,
                              NhaCungCap = c.TenNCC,
                              GiaNhap = b.GiaNhap,
                              TonKho = b.TonKho
                          }
                             ).OrderByDescending(x => x.Soluong).ToPagedList(page, pagesize);
            }
            else
            {
                result = (from a in db.Categories
                          join b in db.Sanphams on a.IDCategory equals b.CategoryID
                          join c in db.NhaCungCaps on b.IDNCC equals c.IDNCC
                          join f in db.Users on b.IDNguoiTao equals f.IDUser
                          where b.Quanlity == "cũ nát"
                          select new SanPhamModel()
                          {
                              IDContent = b.IDContent,
                              Name = b.Name,
                              Metatile = b.MetaTitle,
                              TacGia = b.TacGia,
                              NhaXuatBan = b.NhaXuatBan,
                              Soluong = b.Soluong,
                              Images = b.Images,
                              CategoryID = b.CategoryID,
                              Quanlity = b.Quanlity,
                              NgayTao = b.NgayTao,
                              NguoiTao = f.UserName,
                              Status = b.Status,
                              GiaTien = b.GiaTien,
                              TenTheloai = a.TenTheloai,
                              NhaCungCap = c.TenNCC,
                              GiaNhap = b.GiaNhap,
                              TonKho = b.TonKho
                          }
                             ).OrderByDescending(x => x.Soluong).ToPagedList(page, pagesize);
            }
            return result;
        }
        public IEnumerable<Sanpham> hetHang(string searhString, int page, int pagesize)
        {
            IQueryable<Sanpham> model = db.Sanphams.Where(x=>x.TonKho ==0);
            if (!string.IsNullOrEmpty(searhString))
            {
                model = model.Where(x => x.Name.Contains(searhString) || x.TacGia.Contains(searhString));
                //Contains tìm chuỗi gần đúng
            }
            return model.OrderByDescending(x => x.NgayTao).ToPagedList(page, pagesize);// nhận 2 giá trị page và page size
                                                                                       // cần sắp sếp theo thứ tự ngày tạo
        }
        public IEnumerable<Sanpham> listProduct(string searhString, int page, int pagesize)
        {
            IQueryable<Sanpham> model = db.Sanphams;
            if (!string.IsNullOrEmpty(searhString))
            {
                model = model.Where(x => x.Name.Contains(searhString) || x.TacGia.Contains(searhString));
                //Contains tìm chuỗi gần đúng
            }
            return model.OrderByDescending(x => x.NgayTao).ToPagedList(page, pagesize);// nhận 2 giá trị page và page size
                                                                                       // cần sắp sếp theo thứ tự ngày tạo
        }
        public IEnumerable<SanPhamModel> listTheloai(string searhString,int page,int pagesize)
        {
            dynamic result;
            if (!string.IsNullOrEmpty(searhString))
            {
                 result = (from a in db.Categories
                           join b in db.Sanphams on a.IDCategory equals b.CategoryID
                           where b.Name.Contains(searhString) || b.TacGia.Contains(searhString) || a.TenTheloai.Contains(searhString)
                              select new SanPhamModel()
                              {
                                  IDContent = b.IDContent,
                                  Name = b.Name,
                                  Metatile = b.MetaTitle,
                                  TacGia = b.TacGia,
                                  NhaXuatBan = b.NhaXuatBan,
                                  Soluong = b.Soluong,
                                  Images = b.Images,
                                  CategoryID = b.CategoryID,
                                  Quanlity = b.Quanlity,
                                  NgayTao = b.NgayTao,
                                  Status = b.Status,
                                  GiaTien = b.GiaTien,
                                  TenTheloai = a.TenTheloai,
                                  GiaNhap = b.GiaNhap,
                                  TonKho = b.TonKho
                              }
                              ).OrderByDescending(x => x.NgayTao).ToPagedList(page, pagesize);
            }
            else
            {
                 result = (from a in db.Categories
                           join b in db.Sanphams on a.IDCategory equals b.CategoryID
                          
                           select new SanPhamModel()
                              {
                               IDContent = b.IDContent,
                               Name = b.Name,
                               Metatile = b.MetaTitle,
                               TacGia = b.TacGia,
                               NhaXuatBan = b.NhaXuatBan,
                               Soluong = b.Soluong,
                               Images = b.Images,
                               CategoryID = b.CategoryID,
                               Quanlity = b.Quanlity,
                               NgayTao = b.NgayTao,
                              
                               Status = b.Status,
                               GiaTien = b.GiaTien,
                               TenTheloai = a.TenTheloai,
                              
                               GiaNhap = b.GiaNhap,
                               TonKho = b.TonKho
                           }
                              ).OrderByDescending(x => x.NgayTao).ToPagedList(page, pagesize);
            }
            return result;
        }
        public IEnumerable<SanPhamModel> listKhoHang(string searhString, int page, int pagesize)
        {
            dynamic result;
            if (!string.IsNullOrEmpty(searhString))
            {
                result = (from a in db.Categories
                          join b in db.Sanphams on a.IDCategory equals b.CategoryID
                         
                          where b.Name.Contains(searhString) || b.TacGia.Contains(searhString) || a.TenTheloai.Contains(searhString)
                          select new SanPhamModel()
                          {
                              IDContent = b.IDContent,
                              Name = b.Name,
                              Metatile = b.MetaTitle,
                              TacGia = b.TacGia,
                              NhaXuatBan = b.NhaXuatBan,
                              Soluong = b.Soluong,
                              Images = b.Images,
                              CategoryID = b.CategoryID,
                              Quanlity = b.Quanlity,
                              NgayTao = b.NgayTao,
                              
                              Status = b.Status,
                              GiaTien = b.GiaTien,
                              GiaNhap = b.GiaNhap,
                              TenTheloai = a.TenTheloai,
                              
                              TonKho = b.TonKho
                          }
                             ).OrderByDescending(x => x.NgayTao).ToPagedList(page, pagesize);
            }
            else
            {
                result = (from a in db.Categories
                          join b in db.Sanphams on a.IDCategory equals b.CategoryID
                         
                          select new SanPhamModel()
                          {
                              IDContent = b.IDContent,
                              Name = b.Name,
                              Metatile = b.MetaTitle,
                              TacGia = b.TacGia,
                              NhaXuatBan = b.NhaXuatBan,
                              Soluong = b.Soluong,
                              Images = b.Images,
                              CategoryID = b.CategoryID,
                              Quanlity = b.Quanlity,
                              NgayTao = b.NgayTao,
                             
                              Status = b.Status,
                              GiaTien = b.GiaTien,
                              TenTheloai = a.TenTheloai,
                              
                              GiaNhap = b.GiaNhap,
                              TonKho = b.TonKho
                          }
                             ).OrderByDescending(x => x.NgayTao).ToPagedList(page, pagesize);
            }
            return result;
        }
        public IEnumerable<SanPhamModel> dataThongKe(string fromDate, string toDate)
        {
            
            var query = (from a in db.Categories
                         join b in db.Sanphams on a.IDCategory equals b.CategoryID

                         select new SanPhamModel()
                         {
                             IDContent = b.IDContent,
                             Name = b.Name,
                             Metatile = b.MetaTitle,
                             TacGia = b.TacGia,
                             NhaXuatBan = b.NhaXuatBan,
                             Soluong = b.Soluong,
                             Images = b.Images,
                             CategoryID = b.CategoryID,
                             Quanlity = b.Quanlity,
                             NgayTao = b.NgayTao,

                             Status = b.Status,
                             GiaTien = b.GiaTien,
                             GiaNhap = b.GiaNhap,
                             TenTheloai = a.TenTheloai,

                             TonKho = b.TonKho
                         }
                     ).OrderByDescending(x => x.NgayTao).Take(10000);

            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime start = DateTime.ParseExact(fromDate, "yyyy-MM-dd", CultureInfo.GetCultureInfo("vi-VN"));

                query = query.Where(x => x.NgayTao >= start);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", CultureInfo.GetCultureInfo("vi-VN"));

                query = query.Where(x => x.NgayTao <= endDate);
            }
            var result = query;
            return result.ToList();
        }
        public List<Sanpham> ListName(string keyWord)
        {
            return db.Sanphams.Where(x => x.Name.Contains(keyWord)).Take(6).ToList();
        }
        public List<Sanpham> getAllProduct()
        {
            return db.Sanphams.Where(x => x.Status == true).ToList();
        }
        public List<Sanpham> danhSachSanPhamTon()
        {
            return db.Sanphams.Where(x => x.Status == true && x.TonKho == 0).ToList();
        }
        public List<Sanpham> listSanphamnew(int top)
        {
            return db.Sanphams.OrderByDescending(x => x.NgayTao).Where(x => x.Status == true).Take(top).ToList();
        }
        public List<Sanpham> listDealPrice(int take)
        {
            return db.Sanphams.OrderByDescending(x => x.PriceSale).Where(x => x.Status == true && x.PriceSale != null).Take(take).ToList();
        }
        public List<Sanpham> geyByIdCategory(long idCategory,ref int totalRecord , int pageIndex =1 , int pagesize =2)
        {
            totalRecord = db.Sanphams.Where(x => x.CategoryID == idCategory).Count();
            var model = db.Sanphams.Where(x => x.CategoryID == idCategory).OrderByDescending(x=>x.NgayTao).Skip((pageIndex-1)*pagesize).Take(pagesize).ToList();
            return model;
        }
        public List<Sanpham> getPaginationProduct(ref int totalRecord, int pageIndex = 1, int pagesize = 10)
        {
            
            var model = db.Sanphams.Where(x => x.Status == true).OrderByDescending(x => x.NgayTao).Skip((pageIndex - 1) * pagesize).Take(pagesize).ToList();
            return model;
        }
        public IEnumerable<Sanpham> getByKeyWord(string keyWord,int page, int pageSize)
        {
            IQueryable<Sanpham> model = db.Sanphams.Where(x => x.Name.Contains(keyWord));
            model = model.Where(x => x.Status == true);
            //Contains tìm chuỗi gần đúng
            return model.OrderByDescending(x => x.NgayTao).OrderByDescending(x => x.NgayTao).ToPagedList(page, pageSize);// nhận 2 giá trị page và page size 
            
        }
        public IEnumerable<SanPhamModel> getByIDcate(long idCate,string searhString,int page,int pageSize)
        {
            dynamic result;
            if (!string.IsNullOrEmpty(searhString))
            {
                result = (from a in db.Categories
                          join b in db.Sanphams on a.IDCategory equals b.CategoryID
                          where b.CategoryID == idCate && b.Name.Contains(searhString) || b.CategoryID == idCate && b.TacGia.Contains(searhString) || a.TenTheloai.Contains(searhString)
                          select new SanPhamModel()
                          {
                              IDContent = b.IDContent,
                              Name = b.Name,
                              Metatile = b.MetaTitle,
                              TacGia = b.TacGia,
                              NhaXuatBan = b.NhaXuatBan,
                              Soluong = b.Soluong,
                              Images = b.Images,
                              CategoryID = b.CategoryID,
                              Quanlity = b.Quanlity,
                              NgayTao = b.NgayTao,
                              Status = b.Status,
                              Mota = b.Mota,
                              ChiTiet = b.ChiTiet,
                              TenTheloai = a.TenTheloai,
                              GiaTien = b.GiaTien
                          }
                             ).OrderByDescending(x => x.NgayTao).ToPagedList(page, pageSize);
            }
            else
            {
                result = (from a in db.Categories
                          join b in db.Sanphams on a.IDCategory equals b.CategoryID
                          where b.CategoryID == idCate
                          select new SanPhamModel()
                          {
                              IDContent = b.IDContent,
                              Name = b.Name,
                              Metatile = b.MetaTitle,
                              Mota = b.Mota,
                              ChiTiet = b.ChiTiet,
                              TacGia = b.TacGia,
                              NhaXuatBan = b.NhaXuatBan,
                              Soluong = b.Soluong,
                              Images = b.Images,
                              CategoryID = b.CategoryID,
                              Quanlity = b.Quanlity,
                              NgayTao = b.NgayTao,
                              Status = b.Status,
                              GiaTien = b.GiaTien,
                              TenTheloai = a.TenTheloai,

                          }
                             ).OrderByDescending(x => x.NgayTao).ToPagedList(page, pageSize);
            }
            return result;

        }
        public IEnumerable<Sanpham> getByIDcateDetail(long? idCate,long idProduct)
        {
            IQueryable<Sanpham> model = db.Sanphams.Where(x => x.CategoryID == idCate && x.IDContent != idProduct);
              model = model.Where(x =>x.Status == true);
                //Contains tìm chuỗi gần đúng
            return model.OrderByDescending(x => x.NgayTao).ToList();// nhận 2 giá trị page và page size                                                                           // cần sắp sếp theo thứ tự ngày tạo

        }
        public IEnumerable<Sanpham> getByIDcate(long? idCate, int page,int pageSize)
        {
            IQueryable<Sanpham> model = db.Sanphams.Where(x => x.CategoryID == idCate);
            model = model.Where(x => x.Status == true);
            //Contains tìm chuỗi gần đúng
            return model.OrderByDescending(x => x.NgayTao).OrderByDescending(x => x.NgayTao).ToPagedList(page,pageSize);// nhận 2 giá trị page và page size                                                                           // cần sắp sếp theo thứ tự ngày tạo

        }
        public IEnumerable<SanPhamModel> getByIDCategoryTabHome(long? idcate)
        {
            var result = (from a in db.Categories
                      join b in db.Sanphams on a.IDCategory equals b.CategoryID
                      where b.CategoryID == idcate
                          select new SanPhamModel()
                      {
                          IDContent = b.IDContent,
                          Name = b.Name,
                          Metatile = b.MetaTitle,
                          Mota = b.Mota,
                          ChiTiet = b.ChiTiet,
                          TacGia = b.TacGia,
                          NhaXuatBan = b.NhaXuatBan,
                          Soluong = b.Soluong,
                          Images = b.Images,
                          CategoryID = b.CategoryID,
                          Quanlity = b.Quanlity,
                          NgayTao = b.NgayTao,
                          Status = b.Status,
                          GiaTien = b.GiaTien,
                          PriceSale = b.PriceSale,
                          GiaNhap = b.GiaNhap,
                          TonKho = b.TonKho,
                          TenTheloai = a.TenTheloai,
                      }
                             ).OrderByDescending(x => x.NgayTao).Take(8).ToList();
            return result;
        }
        public IEnumerable<Sanpham> listDelByCategory(long? idCate)
        {
            // cần sắp sếp theo thứ tự ngày tạo
            return db.Sanphams.Where(x => x.CategoryID == idCate).ToList();
        }
        public List<Category> listAllCategoreProduct()
        {
            return db.Categories.Where(x => x.Status == true).ToList();
        }
        public IEnumerable<Sanpham> listAllProduct(int page,int pageSize)
        {
            return db.Sanphams.Where(x => x.Status == true).OrderByDescending(x=>x.NgayTao).ToPagedList(page, pageSize);
        }
        public int ListCount()
        {
            return db.Sanphams.Where(x => x.Status == true).Count();
        }
        public List<Sanpham> listTopSellings(int top)
        {
            return db.Sanphams.Where(x => x.Tophot > 0 && x.Status == true).OrderByDescending(x => x.Tophot).Take(top).ToList();
        }
        
        public List<Sanpham> SanphamLq(long idContent)
        {
            var product = db.Sanphams.Find(idContent);
            return db.Sanphams.Where(x => x.IDContent != idContent && x.CategoryID == product.CategoryID).ToList();
        }

        

        // Lấy danh sách sản phẩm nhiều lượt xem nhất
        public List<Sanpham> SanPhamNhieuLuotXem(int top)
        {
            return db.Sanphams.OrderByDescending(x => x.LuotXem).Take(top).ToList();
        }

        // Lấy sản phẩm liên quan dựa trên thể loại và lượt xem
        public List<Sanpham> SanPhamLQTheoLuotXem(long categoryID, long idContent)
        {
            return db.Sanphams.Where(x => x.CategoryID == categoryID && x.IDContent != idContent)
                              .OrderByDescending(x => x.LuotXem)
                              .Take(5)  // Giới hạn 5 sản phẩm liên quan
                              .ToList();
        }


        // Fetch all products Func getAllProduct

        public void TrainModel()
        {
            var data = db.Sanphams
                .Select(s => new
                {
                    s.IDContent,
                    s.Name,
                    s.TacGia,
                    s.NhaXuatBan,
                    s.GiaTien,
                    s.LuotXem
                })
                .ToList();

            // Now map the anonymous type to your Sanpham class
            var sanphams = data.Select(d => new SanphamRecomend
            {
                IDContent = d.IDContent,
                Name = d.Name,
                TacGia = d.TacGia,
                NhaXuatBan = d.NhaXuatBan,
                GiaTien = d.GiaTien,
                LuotXem = d.LuotXem
            }).ToList();

            var trainingData = mlContext.Data.LoadFromEnumerable(data);
            //var originalData = mlContext.Data.LoadFromEnumerable(db.Sanphams);
            //var selectedData = mlContext.Data.CreateEnumerable<SanphamRecomend>(originalData, reuseRowObject: false);
            //var trainingData = mlContext.Data.LoadFromEnumerable(selectedData);
            var dataProcessPipeline = mlContext.Transforms.Conversion.ConvertType(new[]
                {
                    new InputOutputColumnPair("IDContent", "IDContent"),
                    //new InputOutputColumnPair("CategoryID", "CategoryID"),
                    new InputOutputColumnPair("GiaTien", "GiaTien"),
                    new InputOutputColumnPair("LuotXem", "LuotXem")
                }, DataKind.Single)
                .Append(mlContext.Transforms.Text.FeaturizeText("Name", nameof(SanphamRecomend.Name)))
                .Append(mlContext.Transforms.Text.FeaturizeText("TacGia", nameof(SanphamRecomend.TacGia)))
                .Append(mlContext.Transforms.Text.FeaturizeText("NhaXuatBan", nameof(SanphamRecomend.NhaXuatBan)))
                .Append(mlContext.Transforms.Concatenate("Features", "IDContent", "GiaTien", "LuotXem", "Name", "TacGia", "NhaXuatBan"))
                .Append(mlContext.Regression.Trainers.FastForest(new FastForestRegressionTrainer.Options
                {
                    NumberOfTrees = 4,
                    NumberOfLeaves = 4,
                    FeatureFraction = 1F,
                    LabelColumnName = "LuotXem",
                    FeatureColumnName = "Features"
                }));
            // Train the model
            var model = dataProcessPipeline.Fit(trainingData);

            // Save the model to disk
            mlContext.Model.Save(model, trainingData.Schema, modelPath);
            Console.WriteLine("Model training complete and saved to " + modelPath);

            // Load the prediction engine with the new model
            _predictionEngine = mlContext.Model.CreatePredictionEngine<SanphamRecomend, ProductPrediction>(model);
        }

        // Load the model from file or train if it doesn't exist
        private void LoadModel()
        {
            if (File.Exists(modelPath))
            {
                var model = mlContext.Model.Load(modelPath, out var modelSchema);

                // Tạo SchemaDefinition và cấu hình kiểu dữ liệu cho từng cột
                var schemaDef = SchemaDefinition.Create(typeof(SanphamRecomend));

                // Thiết lập cột số dưới dạng 'Single' để tránh lỗi
                schemaDef["IDContent"].ColumnType = NumberDataViewType.Single;
                schemaDef["GiaTien"].ColumnType = NumberDataViewType.Single;
                schemaDef["LuotXem"].ColumnType = NumberDataViewType.Single;

                // Thiết lập cột văn bản dưới dạng 'TextDataViewType'
                schemaDef["Name"].ColumnType = TextDataViewType.Instance;
                schemaDef["TacGia"].ColumnType = TextDataViewType.Instance;
                schemaDef["NhaXuatBan"].ColumnType = TextDataViewType.Instance;

                // Tạo _predictionEngine bằng cách truyền schemaDef dưới dạng inputSchemaDefinition
                _predictionEngine = mlContext.Model.CreatePredictionEngine<SanphamRecomend, ProductPrediction>(model, inputSchemaDefinition: schemaDef);

                Console.WriteLine("Model loaded from file.");
            }
            else
            {
                Console.WriteLine("Model not found. Training a new model.");
                TrainModel();  // Train a new model if none exists
            }
        }




        public List<SanphamRecomend> Recommendation(long idDetail, int topN)
        {
            if (_predictionEngine == null)
            {
                throw new InvalidOperationException("Prediction engine has not been initialized.");
            }
            // Lấy sản phẩm hiện tại để dự đoán
            var currentProduct = db.Sanphams.FirstOrDefault(p => p.IDContent == idDetail);
            if (currentProduct == null) return new List<SanphamRecomend>();

            var products = db.Sanphams.Where(p => p.IDContent != idDetail).ToList(); // Loại bỏ sản phẩm hiện tại

            var recommendations = products.Select(p =>
            {
                var input = new SanphamRecomend
                {
                    Name = p.Name,
                    MetaTitle = p.MetaTitle,
                    TacGia = p.TacGia,
                    NhaXuatBan = p.NhaXuatBan,
                    Images = p.Images,
                    //CategoryID = p.CategoryID ?? 0,
                    GiaTien = p.GiaTien,
                    //PriceSale = (int)p.PriceSale,
                    LuotXem = p.LuotXem,
                };

                var prediction = _predictionEngine.Predict(input);
                return new { Product = p, prediction.Score };
            })
            .OrderByDescending(r => r.Score)
            .Take(topN)
            .Select(r => new SanphamRecomend
            {
                IDContent = r.Product.IDContent,
                Name = r.Product.Name,
                MetaTitle = r.Product.MetaTitle,
                TacGia = r.Product.TacGia,
                NhaXuatBan = r.Product.NhaXuatBan,
                Images = r.Product.Images,
                //CategoryID = r.Product.CategoryID,
                GiaTien = r.Product.GiaTien,
                //PriceSale = r.Product.PriceSale,
                LuotXem = r.Product.LuotXem
            })
            .ToList();

            return recommendations;
        }


        public bool CheckName(string name)
        {
            var result = db.Sanphams.Count(x => x.Name == name);
            if (result > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public Sanpham getByID(long id)
        {
            return db.Sanphams.Find(id);
        }
        public long Insert(Sanpham model)
        {
             var sp = db.Sanphams.Add(model);
            db.SaveChanges();
            return sp.IDContent;
        }
        public bool Delete(long idContent)
        {
            try
            {
                var sp = db.Sanphams.Find(idContent);
                db.Sanphams.Remove(sp);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CapNhatLuotXem(long idContent, out int luotXem)
        {
            luotXem = 0;
            var sp = db.Sanphams.Find(idContent);
            if (sp != null)
            {
                sp.LuotXem++;
                db.SaveChanges();
                luotXem = sp.LuotXem;
                return true;
            }
            return false;
        }


        public bool Update(Sanpham model)
        {
            try
            {
                var sp = db.Sanphams.Find(model.IDContent);
                sp.Name = model.Name;
                sp.NhaXuatBan = model.NhaXuatBan;
                sp.TacGia = model.TacGia;
                sp.CategoryID = model.CategoryID;
                sp.Images = model.Images;
                sp.MetaTitle = model.MetaTitle;
                
                sp.Status = model.Status;
                sp.Mota = model.Mota;
                sp.ChiTiet = model.ChiTiet;
                sp.Quanlity = model.Quanlity;
                sp.GiaNhap = model.GiaNhap;
                sp.GiaTien = model.GiaTien;
                sp.PriceSale = model.PriceSale;
                db.SaveChanges();
                return true;
            }catch
            {
                return false;
            }
        }

        public bool UpdateSoLuong(long idConten,int soLuong)
        {
            try
            {
                var sp = db.Sanphams.Find(idConten);
                sp.Soluong = soLuong;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool UpdateTonKho(long idConten, int soLuong)
        {
            try
            {
                var sp = db.Sanphams.Find(idConten);
                sp.TonKho -= soLuong;
               
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool UpdateTraHang(long idConten, int soLuong)
        {
            try
            {
                var sp = db.Sanphams.Find(idConten);
                sp.TonKho += soLuong;
                sp.Soluong += soLuong;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool UpdateTopHot(long idConten, int topHot)
        {
            try
            {
                var sp = db.Sanphams.Find(idConten);
                sp.Tophot = topHot;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
