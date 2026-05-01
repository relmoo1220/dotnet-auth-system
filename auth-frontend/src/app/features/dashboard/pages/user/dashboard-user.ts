import { Component } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { IftaLabelModule } from 'primeng/iftalabel';

@Component({
  selector: 'app-dashboard-user',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, PasswordModule, IftaLabelModule],
  templateUrl: './dashboard-user.html',
  styleUrl: './dashboard-user.css',
  standalone: true,
})
export class DashboardUser {}
