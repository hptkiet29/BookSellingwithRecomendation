using BaiTapLon.Models;
using Mood.Draw;
using Mood.EF2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaiTapLon.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }
        [ChildActionOnly]
        public PartialViewResult Category()
        {
            var cart = Session[Common.Constant.CartSession];
            var list = new List<CartItem>();
            if (cart != null)
            {
                list = (List<CartItem>)cart;
            }
            ViewBag.CartList = list.Count;
            
            var model = new CategoryDraw().ListAllCategory(7);
            ViewBag.Sanphamnew = new SanphamDraw().listSanphamnew(8);
            ViewBag.topHot = new SanphamDraw().listTopSellings(8);
            ViewBag.getAllProduct = new SanphamDraw().getAllProduct();
            return PartialView(model);
        }

        [ChildActionOnly]
        public PartialViewResult CategoryMobile()
        {
            ViewBag.listMenu = new MenuDraw().listAll();
            var model = new CategoryDraw().ListAllCategory(7);
            ViewBag.getAllProduct = new SanphamDraw().getAllProduct();
            return PartialView(model);
        }
        public JsonResult ListName(string q)
        {
            var data = new SanphamDraw().ListName(q);
            return Json(new
            {
                data = data,
                status = true
            }, JsonRequestBehavior.AllowGet);//cho phép GET bên Method
        }
        public ActionResult Search(string keyWord, int page = 1, int pagesize = 20)
        {
            var model = new SanphamDraw().getByKeyWord(keyWord, page, pagesize);
            ViewBag.totalKq = model.Count();
            ViewBag.keyWord = keyWord;
            ViewBag.listGoiY = new SanphamDraw().listSanphamnew(5);
            ViewBag.total = new SanphamDraw().ListCount();
            ViewBag.Category = new CategoryDraw().ListAll();
            return View(model);
        }
        public ActionResult ListProduct(long? idCate,int page = 1, int pageSize = 20)
        {
            IEnumerable<Mood.EF2.Sanpham> model;
            if(idCate != null)
            {
                model = new SanphamDraw().getByIDcate(idCate,page, pageSize);
                ViewBag.TenTheLoai = new CategoryDraw().getByID((int)idCate);
            }
            else
            {
                model = new SanphamDraw().listAllProduct(page, pageSize);
            }
            ViewBag.listGoiY = new SanphamDraw().listSanphamnew(5);
            ViewBag.total = new SanphamDraw().ListCount();
            ViewBag.Category = new CategoryDraw().ListAll();
            return View(model);
        }
        private QuanLySachDBContext db = new QuanLySachDBContext();

        [HttpGet]
        public ActionResult CapNhatLuotXem(long idContent)
        {
            SanphamDraw sanPhamDraw = new SanphamDraw(db);
            int luotXem;

            // Gọi phương thức CapNhatLuotXem trong lớp SanPhamDraw
            if (sanPhamDraw.CapNhatLuotXem(idContent, out luotXem))
            {
                return Json(new { success = true, message = "Test Capnhatluotxem thành công", luotXem = luotXem }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Test Capnhatluotxem thất bại" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductViews()
        {
            var views = db.Sanphams.Select(p => new {
                IDContent = p.IDContent,
                LuotXem = p.LuotXem
            }).ToList();

            return Json(views, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detail(long? idDetail)
        {
            var sanPhamDraw = new SanphamDraw(db);
            int luotXem;

            // Cập nhật lượt xem của sản phẩm khi vào trang chi tiết
            sanPhamDraw.CapNhatLuotXem(idDetail.Value, out luotXem);

            // Lấy thông tin sản phẩm
            var model = sanPhamDraw.getByID(idDetail.Value);

            // Kiểm tra nếu sản phẩm không tồn tại
            if (model == null)
            {
                return RedirectToAction("NotFound");
            }

            // Lấy thông tin danh mục và sản phẩm liên quan
            ViewBag.sanPhamCategory = new CategoryDraw().getByID(model.CategoryID.Value);
            //ViewBag.sanPhamLienquan = sanPhamDraw.getByIDcateDetail(model.CategoryID, idDetail);
            var relatedProducts = sanPhamDraw.Recommendation(idDetail.Value, 9); // Top 5 sản phẩm liên quan
            ViewBag.sanPhamLienquan = relatedProducts;
            return View(model);
        }
        public ActionResult NotFound()
        {
            return View();
        }
    }
}