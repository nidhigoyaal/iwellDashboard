import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'iwell Dashboard';

  constructor(private router: Router) {}

  ngOnInit() {
    const token = localStorage.getItem('iw_token');
    if (!token) {
      this.router.navigate(['/login']);
    } else {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000;
      if (Date.now() > expiry) {
        localStorage.removeItem('iw_token');
        this.router.navigate(['/login']);
      }
    }
  }
}
