import {Component, Inject, inject, OnInit} from '@angular/core';
import {FormBuilder, Validators, AbstractControl, ValidationErrors} from '@angular/forms';
import {Observable, of} from 'rxjs';
import {map, delay} from 'rxjs/operators';
import {TableLayoutCreateEndpoint} from '../../../endpoints/table-layout-endpoints/table-layout-create-endpoint';
import {TableLayoutGetPreviewEndpoint} from '../../../endpoints/table-layout-endpoints/table-layout-get-endpoint';
import {CreateTableLayoutDto, GetTableLayoutDto, GetTableLayoutListDto} from '../table-layout-model';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {FileUploadEndpoint} from '../../../endpoints/file-upload-endpoint/file-upload-endpoint';
import {TableLayoutUpdateEndpoint} from '../../../endpoints/table-layout-endpoints/table-layout-update-endpoint';


@Component({
  selector: 'app-table-layout-create-dialog',
  standalone: false,
  templateUrl: './table-layout-create-dialog.html',
  styleUrls: ['./table-layout-create-dialog.css']
})
export class TableLayoutCreateDialog implements OnInit {

  private fb = inject(FormBuilder);
  private tableLayoutCreateEndpoint = inject(TableLayoutCreateEndpoint);
  private tableLayoutGetPreviewEndpoint = inject(TableLayoutGetPreviewEndpoint);
  private tableLayoutUpdateEndpoint = inject(TableLayoutUpdateEndpoint);
  private fileUploadEndpoint = inject(FileUploadEndpoint);

  constructor(
    private ref: MatDialogRef<TableLayoutCreateDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { layout?: GetTableLayoutDto }
  ) {}


  selectedColor ='#ffffff';

  form = this.fb.group({
    name: this.fb.control<string>('', {
      validators: [Validators.required],
      asyncValidators: [this.checkNameExists.bind(this)],
      nonNullable: true
    }),
    description: this.fb.control<string | null>(null),
  })

  loading = false;

  imageUrl: string | null = null;

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    // Optional: show preview instantly
    const reader = new FileReader();
    reader.onload = () => this.imageUrl = reader.result as string;
    reader.readAsDataURL(file);

    // Upload to backend
    this.fileUploadEndpoint.uploadFile(file).subscribe({
      next: res => this.imageUrl = res.url, // Replace preview with backend URL
      error: err => console.error('Upload error', err)
    });
  }



  // async validator must return Promise<ValidationErrors | null> or Observable<ValidationErrors | null>
  checkNameExists(control: AbstractControl): Observable<ValidationErrors | null> {
    const name = control.value?.trim();
    if (!name) return of(null);

    return this.tableLayoutGetPreviewEndpoint.handleAsync().pipe(
      map((result: GetTableLayoutDto[] | { tableLayouts: GetTableLayoutDto[] }) => {
        const tableLayouts = Array.isArray(result) ? result : result.tableLayouts;

        const exists = tableLayouts.some(tableLayout => {
          // Ignore the layout being edited
          if (this.data?.layout && tableLayout.id === this.data.layout.id) return false;

          return tableLayout.name.toLowerCase() === name.toLowerCase();
        });

        return exists ? { nameExists: true } : null;
      })
    );
  }


  cancel() {
    this.ref.close(false);
  }

  save() {
    if (this.form.invalid) return;
    this.loading = true;

    const base = {
      name: this.form.controls.name.value,
      backgroundColor: this.imageUrl ? undefined : this.selectedColor,
      floorImageUrl: this.imageUrl || ''
    };

    // ---------- EDIT MODE ----------
    if (this.data?.layout) {
      const updateDto: GetTableLayoutListDto = {
        id: this.data.layout.id,
        name: base.name,
        backgroundColor: base.backgroundColor,
        floorImageUrl: base.floorImageUrl,
        tables: [] // minimal required field
      };

      this.tableLayoutUpdateEndpoint.handleAsync(updateDto).subscribe({
        next: (res) => {
          this.loading = false;
          // return the updated layout to parent
          this.ref.close({
            ...this.data.layout!,
            ...updateDto
          });
        },
        error: err => {
          this.loading = false;
          console.error('Failed to update', err);
        }
      });


      return;
    }

    // ---------- CREATE MODE ----------
    const createDto: CreateTableLayoutDto = {
      name: base.name,
      backgroundColor: base.backgroundColor,
      floorImageUrl: base.floorImageUrl
    };

    this.tableLayoutCreateEndpoint.handleAsync(createDto).subscribe({
      next: res => {
        this.loading = false;
        this.ref.close(res);
      },
      error: err => {
        this.loading = false;
        console.error('Failed to create', err);
      }
    });
  }





  removeBackgroundImage() {
    this.imageUrl = null;
  }

  ngOnInit() {
    if (this.data?.layout) {
      const L = this.data.layout;

      this.form.patchValue({
        name: L.name
      });

      this.selectedColor = L.backgroundColor ?? '#ffffff';
      this.imageUrl = L.floorImageUrl && L.floorImageUrl !== ''
        ? L.floorImageUrl
        : null;
    }
  }

}
