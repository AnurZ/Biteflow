import {ControlContainer, FormControl, FormGroup, ValidationErrors} from '@angular/forms';
import {Directive} from '@angular/core';

@Directive() // Adds Angular decorator
export abstract class BaseFormControlComponent {
  // Input za for control name
  public myControlName!: string;
  public customMessages: Record<string, string> = {};

  constructor(protected controlContainer: ControlContainer) {
  }

  // Getting formGroup-a iz ControlContainer
  get formGroup(): FormGroup {
    return this.controlContainer.control as FormGroup;
  }

  // Getting formControl-a by name
  get formControl(): FormControl {
    return this.formGroup.get(this.myControlName) as FormControl;
  }

  // Generates list of errors
  getErrorKeys(errors: ValidationErrors | null): string[] {
    return errors ? Object.keys(errors) : [];
  }

  // Generates error messages
  getErrorMessage(errorKey: string, errorValue: any): string {
    if (this.customMessages[errorKey]) {
      return this.customMessages[errorKey];
    }

    const dynamicMessages: { [key: string]: (errorValue: any) => string } = {
      required: () => 'This field is required.',
      min: (value: any) => `Minimum ${value.requiredLength} characters required. You entered ${value.actualLength}.`,
      max: (value: any) => `Maximum ${value.requiredLength} characters allowed. You entered ${value.actualLength}.`,
      pattern: () => 'Invalid format.',
    };

    if (dynamicMessages[errorKey]) {
      return dynamicMessages[errorKey](errorValue);
    }

    return `Validation error: ${errorKey}`;
  }

  // Helper method for generating ID of control
  protected getControlName(): string {
    return Object.keys(this.formGroup.controls).find(
      key => this.formGroup.get(key) === this.formControl
    ) || '';
  }
}
