import { HttpErrorResponse } from '@angular/common/http';
import { extractErrorMessage } from './error.interceptor';

describe('extractErrorMessage', () => {
  it('retorna mensagem de conexão quando status é 0 (backend fora do ar)', () => {
    const err = new HttpErrorResponse({ status: 0 });
    expect(extractErrorMessage(err)).toBe(
      'Não foi possível conectar ao servidor. Verifique se os serviços estão em execução.'
    );
  });

  it('junta as mensagens de um ValidationProblemDetails do ASP.NET', () => {
    const err = new HttpErrorResponse({
      status: 400,
      error: {
        errors: {
          Codigo: ['Código é obrigatório.'],
          Saldo: ['Saldo deve ser maior ou igual a zero.']
        }
      }
    });
    expect(extractErrorMessage(err)).toBe(
      'Código é obrigatório. Saldo deve ser maior ou igual a zero.'
    );
  });

  it('usa a string simples de erro quando o backend retorna BadRequest(string)', () => {
    const err = new HttpErrorResponse({
      status: 400,
      error: 'Saldo insuficiente para PROD1. Disponível: 2, solicitado: 5.'
    });
    expect(extractErrorMessage(err)).toBe(
      'Saldo insuficiente para PROD1. Disponível: 2, solicitado: 5.'
    );
  });

  it('usa fallback genérico quando não há corpo de erro utilizável', () => {
    const err = new HttpErrorResponse({ status: 500, error: null });
    expect(extractErrorMessage(err)).toBe('Ocorreu um erro. Tente novamente.');
  });
});
