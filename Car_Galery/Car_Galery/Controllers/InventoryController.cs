﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Car_Galery.Entities;
using Car_Galery.Helpers;
using Car_Galery.Managers;
using Car_Galery.Managers.Abstract;
using Car_Galery.Models;
using Car_Galery.Models.ViewModels;
using PagedList;
using Type = Car_Galery.Entities.Type;


namespace Car_Galery.Controllers
{
    public class InventoryController : Controller
    {
        private  DbContext db = new ApplicationDbContext();

        private IUnitOfWork unitOfWork;

        // GET: Inventory
        public ActionResult Index(int? PageNumber, int? brandId)
        { 
            unitOfWork = new EFUnitOfWork(db);

            InventoryViewModel ıvm = new InventoryViewModel();

            ıvm.FilterModel = new FilterModel();

            #region Vehicle List Model Binding
            List<VehicleModel> vhList = new List<VehicleModel>();

            vhList = unitOfWork.GetRepository<Vehicle>().GetAll(v=>v.Rentable == false).ProjectTo<VehicleModel>().ToList();

            if (brandId != null)
            {
                vhList = vhList.Where(vh => vh.BrandId == brandId).ToList();
            }

            ıvm.FilterModel.ResultCount = vhList.Count();

            ıvm.PagedVehicleModels = vhList.ToPagedList(PageNumber ?? 1, 6);
            #endregion

            #region Type Model  Brand Binding
            ıvm.TypeModels = unitOfWork.GetRepository<Type>().GetAll().ProjectTo<TypeModel>().ToList();
            ıvm.BrandModels = new List<BrandModel>();
            ıvm.ModelModels = new List<ModelModel>();
            #endregion

            #region Filter Model Binding
            
            ıvm.FilterModel.Filtered = false;
            
            #endregion
            
            #region Brand Categories Model Binding
                ıvm.BrandModelModels = unitOfWork.GetRepository<Brand>().GetAll().ProjectTo<BrandModelsModel>().ToList();

                foreach (var BrandModelModel in ıvm.BrandModelModels)
                {
                    BrandModelModel.brandModels= unitOfWork.GetRepository<Model>()
                        .GetAll(ty => ty.BrandId == BrandModelModel.BrandId).ProjectTo<ModelModel>().ToList();
                }
            #endregion

            unitOfWork.Dispose();
            return View(ıvm);
        }

        public PartialViewResult FilterVehicle(FilterModel fm)
        {
            unitOfWork = new EFUnitOfWork(db);

            InventoryViewModel ıvm = new InventoryViewModel();

            var query = VehicleListHelper.Filter(fm, unitOfWork,false);

            List<VehicleModel> vhList = new List<VehicleModel>();

            vhList = query.ProjectTo<VehicleModel>().ToList();

            fm.ResultCount = vhList.Count();
            
            ıvm.PagedVehicleModels = vhList.ToPagedList(fm.PageNumber ?? 1, 6);

            int k = ıvm.PagedVehicleModels.Count;

            ıvm.FilterModel = fm;
            
            unitOfWork.Dispose();
            return PartialView("_VehicleListPartial", ıvm);
        }

        public PartialViewResult GetVehicleModal(int? id)
        {
            unitOfWork = new EFUnitOfWork(db);
            var modal = unitOfWork.GetRepository<Vehicle>().GetById((int)id);

            VehicleModalViewModel vm = Mapper.Map<Vehicle, VehicleModalViewModel>(modal);

            unitOfWork.Dispose();
            return PartialView("_VehicleModalPartial", vm);
        }

        public PartialViewResult VehicleDetailModal(int? id)
        {
            unitOfWork = new EFUnitOfWork(db);
            var modal = unitOfWork.GetRepository<Vehicle>().GetById((int)id);

            VehicleModalViewModel vm = Mapper.Map<Vehicle, VehicleModalViewModel>(modal);

            unitOfWork.Dispose();
            return PartialView("_VehicleDetailPartialView", vm);
        }

        [Authorize(Roles = "Admin")]
        public PartialViewResult VehicleEditModal(int? id)
        {
            unitOfWork = new EFUnitOfWork(db);
            var modal = unitOfWork.GetRepository<Vehicle>().GetById((int) id);

            VehicleOperationView vov = new VehicleOperationView();

            vov.VehicleModalViewModel = Mapper.Map<Vehicle, VehicleModalViewModel>(modal);

            var types = unitOfWork.GetRepository<Type>().GetAll().Select(t => new SelectListItem()
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            var brands = unitOfWork.GetRepository<Brand>().GetAll().Select(b => new SelectListItem()
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList();

            var models = unitOfWork.GetRepository<Model>().GetAll().Select(m => new SelectListItem()
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            vov.Types = new SelectList(types,"Value","Text",vov.VehicleModalViewModel.TypeId);

            vov.Brands = new SelectList(brands,"Value","Text",vov.VehicleModalViewModel.BrandId);

            vov.Models = new SelectList(models,"Value","Text",vov.VehicleModalViewModel.ModelId);

            unitOfWork.Dispose();

            return PartialView("_VehicleEditPartialView", vov);

        }

        [Authorize(Roles = "Admin")]
        public PartialViewResult VehicleAddModal()
        {
            unitOfWork = new EFUnitOfWork(db);

            VehicleOperationView vov = new VehicleOperationView();

            vov.VehicleModalViewModel = new VehicleModalViewModel();

            var types = unitOfWork.GetRepository<Type>().GetAll().Select(t => new SelectListItem()
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            var brands = unitOfWork.GetRepository<Brand>().GetAll().Select(b => new SelectListItem()
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList();

            var models = unitOfWork.GetRepository<Model>().GetAll().Select(m => new SelectListItem()
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();

            vov.Types = new SelectList(types,"Value","Text",vov.VehicleModalViewModel.TypeId);

            vov.Brands = new SelectList(brands,"Value","Text",vov.VehicleModalViewModel.BrandId);

            vov.Models = new SelectList(models,"Value","Text",vov.VehicleModalViewModel.ModelId);

            unitOfWork.Dispose();

            return PartialView("_VehicleAddPartialView", vov);

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult AddVehicleConfirm(VehicleOperationView vov, HttpPostedFileBase file1)
        {
            if (ModelState.IsValid)
            {
                unitOfWork = new EFUnitOfWork(db);

                var entity = Mapper.Map<VehicleModalViewModel, Vehicle>(vov.VehicleModalViewModel);

                if (file1 != null)
                {
                    entity.ImageUrl = "~/Images/VehicleImages/" + entity.Name + entity.Year + ".png";;
                    string path = Path.Combine(Server.MapPath(entity.ImageUrl));
                    file1.SaveAs(path);
                }

                entity.Rented = false;

                unitOfWork.GetRepository<Vehicle>().Add(entity);

                unitOfWork.SaveChanges();

                unitOfWork.Dispose();
            }
            

            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public PartialViewResult DeleteVehicle(int id)
        {
            unitOfWork = new EFUnitOfWork(db);

            unitOfWork.GetRepository<Vehicle>().Delete(id);

            unitOfWork.SaveChanges();

            InventoryViewModel ıvm = new InventoryViewModel();

            ıvm.FilterModel = new FilterModel();

            List<VehicleModel> vhList = new List<VehicleModel>();

            vhList = unitOfWork.GetRepository<Vehicle>().GetAll(v=>v.Rentable == false).ProjectTo<VehicleModel>().ToList();

            ıvm.FilterModel.ResultCount = vhList.Count();

            ıvm.PagedVehicleModels = vhList.ToPagedList( 1, 6);
            unitOfWork.Dispose();
            return PartialView("_VehicleListPartial", ıvm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult EditVehicleConfirm(VehicleOperationView vov, HttpPostedFileBase file1)
        {
            
            if (ModelState.IsValid)
            {
                unitOfWork = new EFUnitOfWork(db);

                var entity = unitOfWork.GetRepository<Vehicle>().GetById(vov.VehicleModalViewModel.Id);

                string imageUrl = entity.ImageUrl;

                Mapper.Map(vov.VehicleModalViewModel, entity);

                entity.ImageUrl = imageUrl;
                if (file1 != null)
                {
                    string fullPath = Request.MapPath(entity.ImageUrl);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    entity.ImageUrl = "~/Images/VehicleImages/" + entity.Name + ".png";;
                    string path = Path.Combine(Server.MapPath(entity.ImageUrl));
                    file1.SaveAs(path);
                }
            
                unitOfWork.GetRepository<Vehicle>().Update(entity);

                unitOfWork.SaveChanges();

                unitOfWork.Dispose();
            }

            return RedirectToAction("VehicleEditModal", new {id = vov.VehicleModalViewModel.Id});

        }

        public ActionResult FillBrands(int? TypeId)
        {
            unitOfWork = new EFUnitOfWork(db);

            List<BrandModel> brands = new List<BrandModel>();

            brands = unitOfWork.GetRepository<TypeBrand>().GetAll().Where(tb => tb.TypeId == TypeId)
                .Select(tb => tb.Brand).ProjectTo<BrandModel>().ToList();
            unitOfWork.Dispose();
            return Json(brands, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FillModels(int? BrandId)
        {
            unitOfWork = new EFUnitOfWork(db);

            List<ModelModel> models = new List<ModelModel>();

            models = unitOfWork.GetRepository<Model>().GetAll().Where(m => m.BrandId == BrandId).ProjectTo<ModelModel>()
                .ToList();
            unitOfWork.Dispose();
            return Json(models, JsonRequestBehavior.AllowGet);
        }

        
    }
}