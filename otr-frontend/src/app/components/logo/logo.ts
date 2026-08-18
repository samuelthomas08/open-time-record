import { Component, input } from '@angular/core';

@Component({
  selector: 'app-logo',
  templateUrl: './logo.html',
})
export class Logo {
  readonly class = input('h-5 w-auto text-foreground');
}
