import { Component, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroupDirective, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { ProdutoService } from '../../services/produto.service';
import { Produto } from '../../models/produto.model';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatTableModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatCardModule, MatSnackBarModule, MatProgressSpinnerModule
  ],
  templateUrl: './produtos.component.html',
  styleUrl: './produtos.component.scss'
})
export class ProdutosComponent implements OnInit {
  private produtoService = inject(ProdutoService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  @ViewChild(FormGroupDirective) formDirective!: FormGroupDirective;

  produtos: Produto[] = [];
  colunas = ['codigo', 'descricao', 'saldo'];
  carregandoProdutos = false;
  erroAoCarregarProdutos = false;
  salvando = false;
  gerandoSugestao = false;

  form = this.fb.group({
    codigo: ['', Validators.required],
    descricao: ['', Validators.required],
    saldo: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.carregandoProdutos = this.produtos.length === 0;
    this.erroAoCarregarProdutos = false;
    this.produtoService.getAll()
      .pipe(finalize(() => this.carregandoProdutos = false))
      .subscribe({
        next: (produtos) => this.produtos = produtos,
        error: () => this.erroAoCarregarProdutos = true
      });
  }

  sugerirCodigo(): void {
    const descricao = this.form.get('descricao')?.value;
    if (!descricao) {
      this.snackBar.open('Digite uma descrição antes de gerar o código.', 'Fechar', { duration: 3000 });
      return;
    }

    this.gerandoSugestao = true;
    this.produtoService.sugerirCodigo(descricao)
      .pipe(finalize(() => this.gerandoSugestao = false))
      .subscribe({
        next: (res) => this.form.patchValue({ codigo: res.codigo }),
        error: () => this.snackBar.open('Erro ao gerar código com IA.', 'Fechar', { duration: 4000 })
      });
  }

  salvar(): void {
    if (this.form.invalid) return;

    this.salvando = true;
    this.produtoService.create(this.form.value as Produto)
      .pipe(finalize(() => this.salvando = false))
      .subscribe({
        next: () => {
          this.snackBar.open('Produto cadastrado com sucesso!', 'Fechar', { duration: 3000 });
          this.formDirective.resetForm({ saldo: 0 });
          this.carregarProdutos();
        }
      });
  }
}