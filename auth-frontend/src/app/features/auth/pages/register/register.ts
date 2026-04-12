import { Component } from '@angular/core';
import {
  ReactiveFormsModule,
  FormControl,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { IftaLabelModule } from 'primeng/iftalabel';
import { MessageModule } from 'primeng/message';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    IftaLabelModule,
    MessageModule,
  ],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  passwordValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const valid = value.length >= 8 && /[A-Z]/.test(value) && /\d/.test(value);

    return valid ? null : { weakPassword: true };
  }

  matchPassword(group: AbstractControl): ValidationErrors | null {
    const password = group.get('passwordValue')?.value;
    const repeat = group.get('repeatPasswordValue')?.value;

    if (!password || !repeat) return null;

    return password === repeat ? null : { passwordMismatch: true };
  }

  registerForm = new FormGroup(
    {
      usernameValue: new FormControl('', [Validators.required]),
      passwordValue: new FormControl('', [Validators.required, this.passwordValidator]),
      repeatPasswordValue: new FormControl('', [Validators.required]),
    },
    { validators: this.matchPassword },
  );

  register() {
    if (this.registerForm.invalid) return;

    console.log(this.registerForm.value);
  }
}
