import { TestBed } from '@angular/core/testing';
import { ChartLinea } from './chart-linea';

/**
 * Il grafico è SVG calcolato a mano: i due casi che romperebbero il disegno
 * sono la serie vuota e la serie con un punto solo (divisione per zero nel
 * passo orizzontale). La descrizione testuale è l'unico contenuto che uno
 * screen reader riceve, quindi vale la pena verificarla come si verifica un
 * testo, non come un dettaglio estetico.
 */
describe('ChartLinea', () => {
  function crea(punti: { etichetta: string; valore: number }[]) {
    const fixture = TestBed.createComponent(ChartLinea);
    fixture.componentRef.setInput('punti', punti);
    fixture.componentRef.setInput('titolo', 'Visualizzazioni per giorno');
    fixture.componentRef.setInput('unita', 'visualizzazioni');
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('con serie vuota non calcola percorsi e si dichiara vuoto', () => {
    const c = crea([]);

    expect(c.vuoto()).toBe(true);
    expect(c.percorsoLinea()).toBe('');
    expect(c.percorsoArea()).toBe('');
    expect(c.descrizione()).toContain('Nessun dato');
  });

  it('con un solo punto lo centra invece di dividere per zero', () => {
    const c = crea([{ etichetta: '01/09', valore: 4 }]);

    const [primo] = c.coordinate();
    expect(Number.isFinite(primo.x)).toBe(true);
    expect(Number.isFinite(primo.y)).toBe(true);
    expect(primo.x).toBe(300); // metà della larghezza del viewBox (600)
  });

  it('produce un comando per punto e chiude l area', () => {
    const c = crea([
      { etichetta: '01/09', valore: 1 },
      { etichetta: '02/09', valore: 5 },
      { etichetta: '03/09', valore: 3 }
    ]);

    expect(c.percorsoLinea().startsWith('M')).toBe(true);
    expect((c.percorsoLinea().match(/[ML]/g) ?? []).length).toBe(3);
    expect(c.percorsoArea().endsWith('Z')).toBe(true);
  });

  it('serie tutta a zero non produce coordinate non valide', () => {
    const c = crea([
      { etichetta: '01/09', valore: 0 },
      { etichetta: '02/09', valore: 0 }
    ]);

    // Il massimo non scende mai sotto 1: senza, sarebbe una divisione per zero.
    expect(c.valoreMassimo()).toBe(1);
    expect(c.coordinate().every((p) => Number.isFinite(p.y))).toBe(true);
  });

  it('descrive a parole totale e giorno di picco', () => {
    const c = crea([
      { etichetta: '01/09', valore: 2 },
      { etichetta: '02/09', valore: 8 }
    ]);

    const descrizione = c.descrizione();
    expect(descrizione).toContain('10 visualizzazioni');
    expect(descrizione).toContain('02/09');
    expect(c.etichettaIniziale()).toBe('01/09');
    expect(c.etichettaFinale()).toBe('02/09');
  });
});
