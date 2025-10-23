import { Component,Input,OnInit } from '@angular/core';
import {ControlContainer} from '@angular/forms';
import {BaseFormControlComponent} from '../base-form-control-component';

export enum InputTextType {
  Text = 'text',
  Password = 'password',
  Email = 'email',
  Number = 'number',
  Tel = 'tel',
  Url = 'url'
}

@Component({
  selector: 'app-input-text',
  standalone: false,
  templateUrl: './input-text.html',
  styleUrl: './input-text.css'
})
export class InputText extends BaseFormControlComponent implements OnInit{
  @Input() myLabel!: string; // Labela za input
  @Input() myId: string = ''; // ID za input (koristi se u <label> for atributu)
  @Input() myPlaceholder: string = ''; // Placeholder tekst
  @Input() myType: InputTextType = InputTextType.Text; // Tip inputa koristi enumeraciju

  @Input() override customMessages: Record<string, string> = {};
  @Input() override myControlName: string = "";

  constructor(protected override controlContainer: ControlContainer) {
    super(controlContainer);
  }

    ngOnInit(): void {
      if (!this.myId && this.myId === '' && this.formControl) {
        this.myId = this.getControlName();
      }
    }

}
