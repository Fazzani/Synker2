import { PipeTransform, Pipe } from "@angular/core";

@Pipe({ name: 'keys' })
export class KeysPipe implements PipeTransform {
    transform(value, args: string[]): { string, number }[] {
        let keys = [];
        for (var enumMember in value) {
            if (!isNaN(parseInt(enumMember, 10))) {
                keys.push({ value: enumMember, key : value[enumMember] });
            }
        }
        return keys;
    }
}