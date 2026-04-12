import { Component } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { IftaLabelModule } from 'primeng/iftalabel';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, PasswordModule, IftaLabelModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
  standalone: true,
})
export class Login {
  loginForm = new FormGroup({
    usernameValue: new FormControl('', [Validators.required]),
    passwordValue: new FormControl('', [Validators.required]),
  });

  login() {
    if (this.loginForm.invalid) {
      return;
    }

    console.log(this.loginForm.value);
  }
}
