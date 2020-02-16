﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Car_Galery.Entities;
using Car_Galery.Models;
using Type = System.Type;

namespace Car_Galery.App_Start
{
    public static class MappingConfig
    {
        public static void RegisterMaps()
        {
            AutoMapper.Mapper.Initialize(config =>
            {
                config.CreateMap<Vehicle, VehicleModel>();
                config.CreateMap<Model, ModelModel>();
                config.CreateMap<VehicleModel, Vehicle>();
                config.CreateMap<Type, TypeModel>();
                config.CreateMap<TypeModel, Type>();
                config.CreateMap<Brand, BrandModel>();
                config.CreateMap<BrandModel, Brand>();
                config.CreateMap<Brand, BrandModelsModel>()
                    .ForMember(dest => dest.BrandId, opts => opts.MapFrom(src => src.Id))
                    .ForMember(dest => dest.BrandName, opts => opts.MapFrom(src => src.Name));

            });
        }
    }
}