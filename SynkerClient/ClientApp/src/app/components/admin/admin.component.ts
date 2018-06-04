import { Component, EventEmitter, Output } from '@angular/core';

@Component({
    selector: 'admin',
    templateUrl: './admin.component.html'
})
export class AdminComponent {
    @Output() sidenavNotif: EventEmitter<boolean> = new EventEmitter<boolean>();
}