using System;
using System.Collections.Generic;
using Menova.Models;

namespace Menova.Areas.Admin.Models
{
    public class OrderManagementViewModel
    {
        public List<Order> Orders { get; set; }
        public PaginationInfo Pagination { get; set; }
        public string StatusFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SearchQuery { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class OrderDetailViewModel
    {
        public Order Order { get; set; }
        public List<OrderStatusOption> AvailableStatuses { get; set; }
    }

    public class OrderStatusOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class PaginationInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }
} 