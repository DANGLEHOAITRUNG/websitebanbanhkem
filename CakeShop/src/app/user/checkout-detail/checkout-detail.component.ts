import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CakeshopService } from '@app/cakeshop.service';

@Component({
  selector: 'app-checkout-detail',
  templateUrl: './checkout-detail.component.html',
  standalone : false,
  styleUrls: ['./checkout-detail.component.css']
})
export class CheckoutDetailComponent implements OnInit {
  userId: any;
  orders: any[] = [];        // Danh sách đơn hàng
  selectedOrder: any = null; // Chi tiết đơn hàng đang xem
  orderDetails: any[] = [];  // Danh sách sản phẩm trong đơn hàng
  
  // Thêm các biến mới
  searchTerm: string = '';
  statusFilter: string = 'all';
  currentPage: number = 1; // lưu trang hiện tại
  itemsPerPage: number = 5;
  filteredOrders: any[] = []; // mảng sau khi lọc
  isLoading: boolean = true;

  constructor(
    private service: CakeshopService,
    private datePipe: DatePipe
  ) {}

  ngOnInit(): void {
    this.userId = localStorage.getItem("userId");
    if (this.userId) {
      this.loadAllOrders(this.userId);
    }
  }

  loadAllOrders(userId: string) {
    this.isLoading = true;
    this.service.getUserOrders(userId).subscribe(response => {
      if (response.status === 'success') {
        this.orders = response.data; // data là danh sách các order
        this.applyFilters();
      }
      this.isLoading = false;
    });
  }

  viewOrderDetail(orderId: number) {
    this.isLoading = true;
    this.service.getOrderDetails(orderId).subscribe(response => {
      if (response.status === 'success') {
        this.selectedOrder = response.data.orderInfo;
        this.orderDetails = response.data.orderDetails;
        this.isLoading = false;
      }
    });
  }

  getTotalAmount(): number {
    return this.orderDetails.reduce((total, item) => total + item.TotalAmount, 0);
  }

  backToList() {
    this.selectedOrder = null;
    this.orderDetails = [];
  }

  // Các phương thức mới
  applyFilters() {
    this.filteredOrders = this.orders.filter(order => {
      // Lọc theo từ khóa tìm kiếm
      if (this.searchTerm) {
        const searchLower = this.searchTerm.toLowerCase();
        return order.OrderID.toString().includes(searchLower) || 
               (order.Status && order.Status.toLowerCase().includes(searchLower));
      }
      
      return true;
    });
  }
 // thay đổi từ khóa tìm kiếm
  onSearchChange() {
    this.currentPage = 1;
    this.applyFilters();
  }

  onStatusFilterChange() {
    this.currentPage = 1;
    this.applyFilters();
  }

  get paginatedOrders() {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredOrders.slice(startIndex, startIndex + this.itemsPerPage);
  }

  get totalPages() {
    return Math.ceil(this.filteredOrders.length / this.itemsPerPage);
  }

  setPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  getPages() {
    const pages = [];
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  getOrderStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'Pending':
        return 'status-pending';
      case 'Completed':
        return 'status-processing';
      case 'Cancelled':
        return 'status-cancelled';
      default:
        return '';
    }
  }

  getOrderProgress(status: string): number {
    switch (status?.toLowerCase()) {
      case 'pending':
        return 1;
      case 'Completed':
        return 2;
      case 'Cancelled':
        return 3;
      default:
        return 0;
    }
  }

  formatDate(date: string): string {
    if (!date) return '';
    return this.datePipe.transform(date, 'dd/MM/yyyy HH:mm') || '';
  }
}
