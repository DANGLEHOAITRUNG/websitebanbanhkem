import { UserCartComponent } from './user/user-cart/user-cart.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './LoginRegister/login/login.component';
import { RegisterComponent } from './LoginRegister/register/register.component';
import { CustomerManagementComponent } from './admin/customer-management/customer-management.component';
import { HomeComponent } from './user/home/home.component';
import { AdminHomeComponent } from './admin/admin-home/admin-home.component';
import { CategoryManagementComponent } from './admin/category-management/category-management.component';
import { OrderManagementComponent } from './admin/order-management/order-management.component';
import { ProductManagementComponent } from './admin/product-management/product-management.component';
import { CustomerAddComponent } from './admin/customer-add/customer-add.component';
import { CustomerDetailComponent } from './admin/customer-detail/customer-detail.component';
import { CustomerEditComponent } from './admin/customer-edit/customer-edit.component';
import { UserProfileComponent } from './user/user-profile/user-profile.component';
import { UserHeaderComponent } from './user/user-header/user-header.component';
import { UserFooterComponent } from './user/user-footer/user-footer.component';
import { ProductAddComponent } from './admin/product-add/product-add.component';
import { ProductEditComponent } from './admin/product-edit/product-edit.component';
import { MenuComponent } from './user/menu/menu.component';
import { ProductDetailComponent } from './user/product-detail/product-detail.component';
import { AboutComponent } from './about/about.component';
import { ContactComponent } from './contact/contact.component';

import { CheckoutDetailComponent } from './user/checkout-detail/checkout-detail.component';
import { CheckoutComponent } from './user/checkout/checkout.component';


const routes: Routes = [
  // Default redirect to login
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  // Login and Register routes
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'about', component: AboutComponent },
  { path: 'contact', component: ContactComponent },
  // Admin routes
  {path: 'admin/admin-home',
    component: AdminHomeComponent,
    children: [
      { path: 'product-management', component: ProductManagementComponent },
      { path: 'product-add', component: ProductAddComponent },
      { path: 'product-edit/:id', component: ProductEditComponent },
      { path: 'category-management', component: CategoryManagementComponent },
      { path: 'customer-management', component: CustomerManagementComponent },
      { path: 'customer-add', component: CustomerAddComponent },
      { path: 'customer-detail/:id', component: CustomerDetailComponent },
      { path: 'customer-edit/:id', component: CustomerEditComponent },
      { path: 'order-management', component: OrderManagementComponent },
      { path: 'order-edit/:id', component: OrderManagementComponent },
      { path: '', redirectTo: 'product-management', pathMatch: 'full' },
    ]
  },
  // User routes
    { path: 'user/home',
      component: HomeComponent,
      children: [
        { path: 'user-header', component: UserHeaderComponent },
        { path: 'user-footer', component: UserFooterComponent },
        { path: 'menu', component: MenuComponent },
      ]
    },
    
    // Route chi tiết sản phẩm ngoài route con của 'home'
    { path: 'product-detail/:id', component: ProductDetailComponent },
    { path: 'user-cart', component: UserCartComponent },
    { path: 'checkout', component: CheckoutComponent },
    { path: 'checkout-detail', component: CheckoutDetailComponent },
    { path: 'user-profile/:id', component: UserProfileComponent },
  ];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
