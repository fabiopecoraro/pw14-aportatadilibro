/**
 * Intervallo accettato per l'anno di pubblicazione di un'opera.
 *
 * Rispecchia `CatalogoConstants` lato backend: il server resta l'unica
 * autorita' sul vincolo, ma replicarlo qui permette di segnalare un refuso
 * nel form prima di spendere un giro di rete. I due valori vanno tenuti
 * allineati se la regola cambia.
 */
export const ANNO_PUBBLICAZIONE_MINIMO = 1450;

/**
 * Un anno di margine sull'anno corrente: le edizioni in uscita riportano
 * spesso l'anno successivo in copertina.
 */
export function annoPubblicazioneMassimo(): number {
  return new Date().getFullYear() + 1;
}
