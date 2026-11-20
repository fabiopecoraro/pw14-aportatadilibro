// Specchio TypeScript degli enum definiti in APortataDiLibro.Core.Enums,
// serializzati dal backend come stringa (HasConversion<string>()).

export enum RuoloUtente {
  Utente = 'Utente',
  Admin = 'Admin'
}

export enum StatoCopia {
  Disponibile = 'Disponibile',
  InPrestito = 'InPrestito'
}

export enum StatoPrestito {
  Richiesto = 'Richiesto',
  Accettato = 'Accettato',
  Rifiutato = 'Rifiutato',
  Completato = 'Completato',
  Annullato = 'Annullato'
}

export enum StatoReport {
  Aperto = 'Aperto',
  Chiuso = 'Chiuso'
}

export enum StatoProposta {
  InAttesa = 'InAttesa',
  Approvata = 'Approvata',
  Rifiutata = 'Rifiutata'
}
