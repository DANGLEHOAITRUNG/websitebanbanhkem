import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
export interface ChangePasswordDto {
  userId: number;
  oldPassword: string;
  newPassword: string;
}
export interface OrderDetail {
  productID: number;
  quantity: number;
  price: number;
}

export interface Order {
  orderID?: number;
  userID: number;
  orderDate: string;
  totalAmount: number;
  status: string;
  orderDetails: OrderDetail[];
}

@Injectable({
  providedIn: 'root'
})
export class CakeshopService {
  private isLoggedInSource = new BehaviorSubject<boolean>(false);
  isLoggedIn$ = this.isLoggedInSource.asObservable();
  private apiUrl: string = 'http://localhost:5130/api';
  private cartCount = new BehaviorSubject<number>(0);
  cartCount$ = this.cartCount.asObservable();

  constructor(private http: HttpClient) {}
  // Login & Register
  updateLoginStatus(status: boolean) {
    console.log('Cập nhật trạng thái đăng nhập:', status);
    this.isLoggedInSource.next(status);
  }
  
  
  dangNhap(nDung: any):Observable<any[]>{
    return this.http.post<any>(this.apiUrl+'/Users/dangnhap', nDung);
  }
  
  dangKy(dky: any):Observable<any[]>{
    return this.http.post<any>(this.apiUrl+'/ReUsers/dangky', dky);
  }
  
// Khach Hang

getAllCustomers(): Observable<any[]> {
  return this.http.get<any>(this.apiUrl+'/Customers');
}

getCustomersById(id: number): Observable<any> {
  return this.http.get(`${this.apiUrl}/Customers/${id}`);
}

addCustomer(customer: any): Observable<any> {
  return this.http.post<any>(this.apiUrl+'/Customers',customer);
}

updateCustomer(id: number, customer: any): Observable<any> {
  return this.http.put(`${this.apiUrl}/Customers/${id}`, customer);
}
// nguoi dung
updateUserProfile(userId: number, data: any) {
  return this.http.put(`${this.apiUrl}/Customers/user-profile/${userId}`, data);
}

deleteCustomer(id: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/Customers/${id}`);
}
 // Đổi mật khẩu khách hàng
 changePassword(data: ChangePasswordDto) {
  return this.http.post(`${this.apiUrl}/Customers/change-password`, data);
}


//San Pham

getAllProducts(): Observable<any[]> {
  return this.http.get<any>(this.apiUrl+'/Products');
}

getProductById(id: number): Observable<any> {
  return this.http.get(`${this.apiUrl}/Products/${id}`);
}

addProduct(product: any): Observable<any> {
  return this.http.post<any>(this.apiUrl+'/Products',product);
}

updateProduct(id: number, product: any): Observable<any> {
  return this.http.put(`${this.apiUrl}/Products/${id}`, product);
}

deleteProduct(id: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/Products/${id}`);
}

searchProducts(name: string): Observable<any> {
  return this.http.get(`${this.apiUrl}/Products/search?name=${encodeURIComponent(name)}`);
}

getProductsByCategory(categoryName: string): Observable<any[]> {
  return this.http.get<any[]>(`${this.apiUrl}/Products/category/${categoryName}`);
}


//Don Hang và giỏ hàng
//  Giỏ Hàng
addToCart(userId: number, productId: number, quantity: number): Observable<any> {
  const body = { UserID: userId, ProductID: productId, Quantity: quantity };
  return this.http.post<any>(`${this.apiUrl}/Carts/AddToCart`, body);
}

//  Lấy giỏ hàng từ API
getCart(userId: number): Observable<any> {
  return this.http.get<any>(`${this.apiUrl}/Carts/GetCart/${userId}`);
}

//  Cập nhật số lượng giỏ hàng từ server
setCartCount(count: number) {
  this.cartCount.next(count);
}

// Gọi API để lấy lại số lượng giỏ hàng từ backend
refreshCartCount(userId: number) {
  this.getCart(userId).subscribe(response => {
    if (response.status === 'success') {
      const count = response.data.length;
      this.setCartCount(count);
    }
  });
}

//  Cập nhật số lượng sản phẩm
updateCart(userId: number, productId: number, quantity: number): Observable<any> {
  return this.http.put(`${this.apiUrl}/Carts/UpdateCart`, { userID: userId, productID: productId, quantity: quantity });
}

//  Xóa sản phẩm khỏi giỏ hàng
DeleteFromCart(userId: number, productId: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/Carts/DeleteFromCart/${userId}/${productId}`);
}
//  Cập nhật trạng thái giỏ hàng
updateCartCount(count: number) {
  this.cartCount.next(count);
}
// Đơn Hàng
// API cho người dùng: Tạo đơn hàng
createOrder(userId: number,orderData:any): Observable<any> {
  return this.http.post(`${this.apiUrl}/Orders/CreateOrder/${userId}`, orderData);
}

// API cho người dùng: Lấy lịch sử đơn hàng
 // 1. Lấy danh sách đơn hàng theo userId
 getUserOrders(userId: string): Observable<any> {
  return this.http.get(`${this.apiUrl}/Orders/UserOrders/List/${userId}`);
}

// 2. Lấy chi tiết sản phẩm trong đơn hàng theo orderId
getOrderDetails(orderId: number): Observable<any> {
  return this.http.get(`${this.apiUrl}/Orders/OrderDetails/${orderId}`);
}

// API cho admin: Lấy tất cả đơn hàng
getAllOrders(): Observable<any> {
  return this.http.get(`${this.apiUrl}/Orders/AdminOrders`);
}

// API cho admin: Cập nhật trạng thái đơn hàng
updateOrderStatus(orderId: number, status: string): Observable<any> {
  return this.http.put(`${this.apiUrl}/Orders/UpdateStatus`, { OrderID: orderId, Status: status });
}

//Danh Mục
getAllCategories(): Observable<any> {
  return this.http.get<any>(this.apiUrl+'/Categories');
}

// Lấy danh mục theo ID
getCategoryById(id: number): Observable<any> {
  return this.http.get(`${this.apiUrl}/Categories/${id}`);
}

// Thêm danh mục
addCategory(category: any): Observable<any> {
  return this.http.post<any>(this.apiUrl+'/Categories',category);
}

// Cập nhật danh mục
updateCategory(id: number, category: any): Observable<any> {
  return this.http.put(`${this.apiUrl}/Categories/${id}`, category);
}

// Xóa danh mục
deleteCategory(id: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/Categories/${id}`);
}

//netstat -ano | findstr :4400
//taskkill /PID 19476 /F
getLoggedInUser() {
  const userData = localStorage.getItem('user');
  return userData ? JSON.parse(userData) : null;
}

logout() {
  localStorage.removeItem('user');
}
}
