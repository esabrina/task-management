import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Task, TaskRequest, TaskStatus } from '../../core/models/task.model';
import { TaskService } from '../../core/services/task.service';

@Component({
  selector: 'app-task-modal',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './task-modal.component.html'
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
