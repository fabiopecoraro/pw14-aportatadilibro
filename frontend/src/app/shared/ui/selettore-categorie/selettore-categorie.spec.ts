import { TestBed } from '@angular/core/testing';
import { SelettoreCategorie } from './selettore-categorie';

/**
 * Il selettore contiene l'unica logica non banale del componente: il tetto
 * massimo di categorie e la distinzione fra "non selezionabile perché il
 * limite è pieno" e "già selezionata, quindi sempre togliibile". Sbagliare la
 * seconda bloccherebbe l'utente a 5 categorie senza modo di correggerle.
 */
describe('SelettoreCategorie', () => {
  const categorie = [
    { id: '1', nome: 'Narrativa' },
    { id: '2', nome: 'Classici' },
    { id: '3', nome: 'Giallo' }
  ];

  function crea(massimo = 5) {
    const fixture = TestBed.createComponent(SelettoreCategorie);
    fixture.componentRef.setInput('categorie', categorie);
    fixture.componentRef.setInput('massimo', massimo);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('aggiunge una categoria non ancora selezionata', () => {
    const c = crea();

    c.commuta('1');

    expect(c.selezione()).toEqual(['1']);
    expect(c.selezionata('1')).toBe(true);
  });

  it('toglie una categoria già selezionata', () => {
    const c = crea();
    c.commuta('1');

    c.commuta('1');

    expect(c.selezione()).toEqual([]);
  });

  it('non supera il massimo consentito', () => {
    const c = crea(2);

    c.commuta('1');
    c.commuta('2');
    c.commuta('3');

    expect(c.selezione()).toEqual(['1', '2']);
    expect(c.limiteRaggiunto()).toBe(true);
  });

  it('al limite disabilita solo le voci non selezionate', () => {
    const c = crea(2);
    c.commuta('1');
    c.commuta('2');

    // Le già scelte restano cliccabili, altrimenti non si potrebbe correggere.
    expect(c.disabilitata('1')).toBe(false);
    expect(c.disabilitata('3')).toBe(true);
  });

  it('segnala il vocabolario vuoto', () => {
    const fixture = TestBed.createComponent(SelettoreCategorie);
    fixture.componentRef.setInput('categorie', []);
    fixture.detectChanges();

    expect(fixture.componentInstance.vuoto()).toBe(true);
  });
});
