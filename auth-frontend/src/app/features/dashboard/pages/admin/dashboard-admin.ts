import { Component } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { IftaLabelModule } from 'primeng/iftalabel';

@Component({
  selector: 'app-dashboard-admin',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, PasswordModule, IftaLabelModule],
  templateUrl: './dashboard-admin.html',
  styleUrl: './dashboard-admin.css',
  standalone: true,
})
export class DashboardAdmin {}
