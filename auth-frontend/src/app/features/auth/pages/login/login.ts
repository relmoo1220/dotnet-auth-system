import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { IftaLabelModule } from 'primeng/iftalabel';
import { AuthService } from '../../auth.service';

@Component({
  selector: 'app-login',
  imports: [
    RouterModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    IftaLabelModule,
  ],
  templateUrl: './login.html',
  styleUrl: './login.css',
  standalone: true,
})
export class Login {
  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}

  loginForm = new FormGroup({
    usernameValue: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    passwordValue: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  login() {
    if (this.loginForm.invalid) return;

    const { usernameValue, passwordValue } = this.loginForm.getRawValue();

    this.auth.login(usernameValue, passwordValue).subscribe({
      next: () => {
        const role = this.auth.getUserRole();
        if (role === 'admin') this.router.navigate(['/dashboard/admin']);
        else this.router.navigate(['/dashboard/user']);
      },
      error: () => {
        alert('Login failed');
      },
    });
  }
}
