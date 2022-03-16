using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tuan4.Models;
using System.Windows.Forms;

namespace Tuan4.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        MyDataDataContext data =new MyDataDataContext();
        public List<Giohang> Laygiohang()
        {
            List<Giohang> lstGiohang = Session["Giohang"] as List<Giohang>;
            if (lstGiohang == null)
            {
                lstGiohang = new List<Giohang>();
                Session["Giohang"] = lstGiohang;
            }
            return lstGiohang;
        }
        public ActionResult ThemGioHang(int id,string strURL)
        {
            List<Giohang> lstGiohang = Laygiohang();
            Giohang sanpham = lstGiohang.Find(n => n.masach == id);
            if (sanpham == null)
            {
                sanpham = new Giohang(id);
                lstGiohang.Add(sanpham);
                return Redirect(strURL);
            }
            else
            {
                sanpham.iSoluong++;
                return Redirect(strURL);
            }
        }
        public int TongSoLuong()
        {
            int tsl = 0;
            List<Giohang> lstGiohang = Session["GioHang"] as List<Giohang>;
            if (lstGiohang != null)
            {
                tsl = lstGiohang.Sum(n => n.iSoluong);
            }
            return tsl;
        }
        private int TongSoLuongSanPham()
        {
            int tsl = 0;
            List<Giohang> lstGiohang = Session["GioHang"] as List<Giohang>;
            if (lstGiohang != null)
            {
                tsl = lstGiohang.Count;
            }
            return tsl;
        }
        private double TongTien()
        {
            double tt = 0;
            List<Giohang> lstGioHang = Session["GioHang"] as List<Giohang>;
            if (lstGioHang != null)
            {
                tt = lstGioHang.Sum(n => n.dThanhtien);
            }
            return tt;
        }
        public ActionResult GioHang()
        {
            List<Giohang> lstGioHang = Laygiohang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            ViewBag.Tongsoluongsanpham = TongSoLuongSanPham();
            return View(lstGioHang);

        }
        public ActionResult GioHangPartial()
        {
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            ViewBag.Tongsoluongsanpham = TongSoLuongSanPham();
            return PartialView();
        }
        public ActionResult XoaGiohang(int id)
        {
            List<Giohang> lstGiohang = Laygiohang();
            Giohang sanpham = lstGiohang.SingleOrDefault(n => n.masach == id);
            if (sanpham != null)
            {
                lstGiohang.RemoveAll(n => n.masach == id);
                return RedirectToAction("GioHang");
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult CapNhapGioHang(int id, System.Web.Mvc.FormCollection collection )
        {
            List<Giohang> lstGioHang = Laygiohang();
            var sach = data.Saches.FirstOrDefault(p => p.masach == id);
            Giohang sanpham = lstGioHang.SingleOrDefault(p => p.masach == id);
            if (sanpham != null)
            {
                sanpham.iSoluong = int.Parse(collection["txtSoLg"].ToString().Trim());
                if (sanpham.iSoluong > sach.soluongton)
                {
                    MessageBox.Show("Không còn đủ sách để bán");
                    sanpham.iSoluong = 1;
                }
            }
            return RedirectToAction("GioHang");
        }
        public ActionResult XoaTatCaGioHang()
        {
            List<Giohang> lstGiohang = Laygiohang();
            lstGiohang.Clear();
            return RedirectToAction("GioHang");
        }
        public ActionResult Dathang()
        {
            List<Giohang> lstGioHang = Laygiohang();
            if (lstGioHang.Count() != 0)
            {
                DialogResult result = MessageBox.Show("bạn muốn đặt hàng", "Hỏi", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    //create invoice
                    Invoice invoice = new Invoice();
                    invoice.Invoice_DateCreate = DateTime.Now;
                    data.Invoices.InsertOnSubmit(invoice);
                    data.SubmitChanges();
                    int invoide_id = data.Invoices.OrderByDescending(p => p.Invoice_ID).Select(p => p.Invoice_ID).FirstOrDefault();
                    //add invoice's detail
                    Invoice_Detail idetail;
                    foreach (var ele in lstGioHang)
                    {
                        idetail = new Invoice_Detail();
                        idetail.masach = ele.masach;
                        idetail.Invoice_ID = invoide_id;
                        idetail.giamua = ele.giaban;
                        idetail.soluong = ele.iSoluong;
                        data.Invoice_Details.InsertOnSubmit(idetail);
                        var book = data.Saches.FirstOrDefault(p => p.masach == ele.masach);
                        book.soluongton -= idetail.soluong;
                        UpdateModel(book);
                    }
                    data.SubmitChanges();
                    string str = "";
                    int i = 1;
                    foreach (var ele in lstGioHang)
                    {
                        str += i + " - " + ele.tensach + "\n";
                        i++;
                    }
                    MessageBox.Show("Đặt hàng thành công!\n" + "---------------\n" + "Danh sách đặt hàng\n" + str);
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                MessageBox.Show("Giỏ hàng trống");
            }
            return RedirectToAction("Index", "Home");

        }
    }
}