import { allargaAllaGriglia, contenutoNellArea } from './geo';

/**
 * L'area inviata al server e' il punto in cui la promessa di privacy della
 * mappa diventa codice: se l'allargamento alla griglia sbagliasse verso
 * l'interno si perderebbero Hub visibili, e se lasciasse passare il
 * rettangolo esatto il suo centro tornerebbe a coincidere con chi guarda.
 */
describe('allargaAllaGriglia', () => {
  const area = { sud: 40.6743, ovest: 14.7512, nord: 40.6861, est: 14.7718 };

  it("contiene per intero l'area di partenza", () => {
    const allargata = allargaAllaGriglia(area);

    expect(allargata.sud).toBeLessThanOrEqual(area.sud);
    expect(allargata.ovest).toBeLessThanOrEqual(area.ovest);
    expect(allargata.nord).toBeGreaterThanOrEqual(area.nord);
    expect(allargata.est).toBeGreaterThanOrEqual(area.est);
  });

  it('aggancia i quattro lati ai multipli del passo', () => {
    expect(allargaAllaGriglia(area)).toEqual({ sud: 40.67, ovest: 14.75, nord: 40.69, est: 14.78 });
  });

  it('da due posizioni vicine produce la stessa area, quindi lo stesso centro', () => {
    const spostata = { sud: 40.6748, ovest: 14.7519, nord: 40.6866, est: 14.7725 };

    expect(allargaAllaGriglia(spostata)).toEqual(allargaAllaGriglia(area));
  });

  it('lascia invariata un\'area gia\' sulla griglia, senza scivolare di un passo', () => {
    const sullaGriglia = { sud: 40.67, ovest: 14.75, nord: 40.69, est: 14.78 };

    expect(allargaAllaGriglia(sullaGriglia)).toEqual(sullaGriglia);
  });

  it('non esce dai limiti del globo', () => {
    const tutto = allargaAllaGriglia({ sud: -89.999, ovest: -179.999, nord: 89.999, est: 179.999 });

    expect(tutto).toEqual({ sud: -90, ovest: -180, nord: 90, est: 180 });
  });
});

describe('contenutoNellArea', () => {
  const area = { sud: 40.67, ovest: 14.75, nord: 40.69, est: 14.78 };

  it('accetta un punto interno e uno sul bordo', () => {
    expect(contenutoNellArea({ lat: 40.68, lng: 14.76 }, area)).toBe(true);
    expect(contenutoNellArea({ lat: 40.67, lng: 14.75 }, area)).toBe(true);
  });

  it('rifiuta un punto appena fuori', () => {
    expect(contenutoNellArea({ lat: 40.691, lng: 14.76 }, area)).toBe(false);
  });
});
