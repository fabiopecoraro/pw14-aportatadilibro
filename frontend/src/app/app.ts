import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/** Id del <main> di ogni schermata, bersaglio dello skip link. */
const ID_CONTENUTO = 'contenuto-principale';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  /**
   * Sposta il focus sul contenuto principale.
   *
   * L'`href="#contenuto-principale"` resta nel markup perche' e' la semantica
   * corretta del collegamento, ma il salto non viene lasciato al
   * comportamento nativo del frammento: in un'applicazione con routing lato
   * client l'ancora finisce per interagire con il router e con la history, e
   * soprattutto il browser sposta il focus solo se il bersaglio e' gia'
   * focalizzabile. Farlo qui esplicitamente rende il comportamento identico su
   * ogni rotta e indipendente da entrambe le cose.
   */
  vaiAlContenuto(evento: Event): void {
    const contenuto = document.getElementById(ID_CONTENUTO);
    if (!contenuto) {
      return;
    }

    evento.preventDefault();
    contenuto.focus();
    contenuto.scrollIntoView({ block: 'start' });
  }
}
