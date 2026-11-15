import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Task, TaskRequest, TaskStatus } from '../../core/models/task.model';
import { TaskService } from '../../core/services/task.service';

@Component({
  selector: 'app-task-modal',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="app-modal-backdrop" role="presentation" (click)="close()">
      <section class="card app-modal" role="dialog" aria-modal="true"
        aria-labelledby="task-modal-title" (click)="$event.stopPropagation()">
        <div class="card-header bg-white d-flex justify-content-between align-items-center px-4 py-3">
          <h2 id="task-modal-title" class="h5 mb-0">{{ task ? 'Task details' : 'Create task' }}</h2>
          <button type="button" class="btn-close" aria-label="Close" (click)="close()"></button>
        </div>

        <div class="card-body p-4">
          @if (errorMessage) {
            <div class="alert alert-danger" role="alert">{{ errorMessage }}</div>
          }
          @if (statusError) {
            <div class="alert alert-danger" role="alert">{{ statusError }}</div>
          }

          <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
            <div class="mb-3">
              <label for="title" class="form-label">Title <span class="text-danger">*</span></label>
              <input id="title" type="text" class="form-control" formControlName="title"
                [attr.maxlength]="task ? 100 : 200" [class.is-invalid]="showError('title')">
              @if (showError('title')) {
                <div class="invalid-feedback">
                  {{ form.controls.title.hasError('required') ? 'Title is required.' : 'Title is too long.' }}
                </div>
              }
            </div>

            <div class="mb-3">
              <label for="description" class="form-label">Description</label>
              <textarea id="description" rows="4" class="form-control"
                formControlName="description" [attr.maxlength]="task ? 1000 : 4000"></textarea>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-sm-6">
                <label for="status" class="form-label">Status <span class="text-danger">*</span></label>
                <select id="status" class="form-select" formControlName="statusId"
                  [class.is-invalid]="showError('statusId')">
                  <option [ngValue]="0" disabled>Select a status</option>
                  @for (status of statuses; track status.id) {
                    <option [ngValue]="status.id">{{ status.name }}</option>
                  }
                </select>
                @if (showError('statusId')) {
                  <div class="invalid-feedback">Select a status.</div>
                }
              </div>
              <div class="col-sm-6">
                <label for="dueDate" class="form-label">Due date <span class="text-danger">*</span></label>
                <input id="dueDate" type="date" class="form-control" formControlName="dueDate"
                  [class.is-invalid]="showError('dueDate')">
                @if (showError('dueDate')) {
                  <div class="invalid-feedback">
                    {{ form.controls.dueDate.hasError('required') ? 'Due date is required.' : 'Enter a valid due date.' }}
                  </div>
                }
              </div>
            </div>

            <div class="d-flex justify-content-end gap-2">
              <button type="button" class="btn btn-outline-secondary" (click)="close()">Cancel</button>
              <button type="submit" class="btn btn-primary"
                [disabled]="isSaving || !!statusError">
                {{ isSaving ? 'Saving…' : task ? 'Save changes' : 'Create task' }}
              </button>
            </div>
          </form>
        </div>
      </section>
    </div>
  `
})
export class TaskModalComponent implements OnInit {
  @Input() task: Task | null = null;
  @Output() readonly closed = new EventEmitter<void>();
  @Output() readonly saved = new EventEmitter<string>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  readonly form = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    description: [''],
    statusId: [0, [Validators.required, Validators.min(1)]],
    dueDate: ['', [Validators.required, Validators.pattern(/^\d{4}-\d{2}-\d{2}$/)]]
  });
  statuses: TaskStatus[] = [];
  statusError = '';
  errorMessage = '';
  isSaving = false;
  private submitted = false;

  ngOnInit(): void {
    this.form.controls.title.setValidators([
      Validators.required,
      Validators.maxLength(this.task ? 100 : 200)
    ]);
    this.form.controls.description.setValidators([
      Validators.maxLength(this.task ? 1000 : 4000)
    ]);
    this.form.controls.title.updateValueAndValidity({ emitEvent: false });
    this.form.controls.description.updateValueAndValidity({ emitEvent: false });

    if (this.task) {
      this.form.patchValue({
        title: this.task.title,
        description: this.task.description ?? '',
        statusId: this.task.statusId,
        dueDate: this.task.dueDate.slice(0, 10)
      });
    }

    this.taskService.getStatuses().subscribe({
      next: (statuses) => {
        this.statuses = statuses;
        if (statuses.length === 0) {
          this.statusError = 'No task statuses are available. Please try again later.';
        }
      },
      error: () => {
        this.statusError = 'Unable to load task statuses. Please close and try again.';
      }
    });
  }

  showError(controlName: 'title' | 'statusId' | 'dueDate'): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || this.submitted);
  }

  close(): void {
    this.closed.emit();
  }

  submit(): void {
    this.submitted = true;
    this.errorMessage = '';
    if (this.form.invalid || this.isSaving || this.statusError) {
      this.form.markAllAsTouched();
      return;
    }

    const formValue = this.form.getRawValue();
    const [year, month, day] = formValue.dueDate.split('-').map(Number);
    const dueDate = new Date(Date.UTC(year, month - 1, day));
    if (
      !Number.isFinite(dueDate.getTime())
      || dueDate.getUTCFullYear() !== year
      || dueDate.getUTCMonth() !== month - 1
      || dueDate.getUTCDate() !== day
    ) {
      this.form.controls.dueDate.setErrors({ date: true });
      this.form.controls.dueDate.markAsTouched();
      return;
    }

    const request: TaskRequest = {
      title: formValue.title.trim(),
      description: formValue.description.trim() || null,
      statusId: formValue.statusId,
      dueDate: dueDate.toISOString()
    };
    if (!request.title) {
      this.form.controls.title.setErrors({ required: true });
      this.form.controls.title.markAsTouched();
      return;
    }

    this.isSaving = true;
    const operation = this.task
      ? this.taskService.updateTask(this.task.id, request)
      : this.taskService.createTask(request);

    operation.subscribe({
      next: () => {
        this.isSaving = false;
        this.saved.emit(this.task ? 'Task updated successfully.' : 'Task created successfully.');
      },
      error: (error: unknown) => {
        this.isSaving = false;
        this.errorMessage = error instanceof HttpErrorResponse && error.status === 404
          ? 'This task was not found or is no longer available.'
          : 'Unable to save this task. Check the information and try again.';
      }
    });
  }
}
