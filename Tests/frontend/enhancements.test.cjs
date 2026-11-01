const {test}=require('node:test');
const assert=require('node:assert/strict');
const {JSDOM}=require('jsdom');
const fs=require('node:fs');
const path=require('node:path');
const source=fs.readFileSync(path.join(__dirname,'../../wwwroot/js/enhancements.js'),'utf8');
async function setup() {
  const rows=Array.from({length:25},(_,i)=>`<tr><td>Room ${i+1}</td><td>${i%2?'Full':'Available'}</td><td><a href='/details/${i+1}'>View</a></td></tr>`).join('');
  const dom=new JSDOM(`<main><div class='table-responsive'><table data-enhance-table><thead><tr><th>Room</th><th>Status</th><th>Actions</th></tr></thead><tbody>${rows}</tbody></table></div><form method='post'><textarea data-val-length-max='500'></textarea><button type='submit' name='status' value='Take'>Save</button></form><div class='alert-danger'>Error</div><div class='alert-success'>Saved</div></main>`,{runScripts:'outside-only',url:'https://localhost'});
  await new Promise(resolve=>dom.window.addEventListener('DOMContentLoaded',resolve,{once:true}));
  dom.window.eval(source);
  return dom;
}
const visible=dom=>Array.from(dom.window.document.querySelectorAll('tbody tr')).filter(r=>!r.hidden&&!r.querySelector('[colspan]'));
test('pagination shows ten rows and next/previous change visible records',async()=>{
 const dom=await setup();try {
 assert.equal(visible(dom).length,10);
 const [prev,next]=dom.window.document.querySelectorAll('.hub-table-pages button');
 assert.equal(prev.disabled,true);next.click();assert.match(visible(dom)[0].textContent,/Room 11/);
 next.click();assert.equal(visible(dom).length,5);assert.equal(next.disabled,true);prev.click();assert.equal(visible(dom).length,10);
 }finally{dom.window.close();}
});
test('search is case-insensitive, updates count, and clear restores records',async()=>{
 const dom=await setup();try{const input=dom.window.document.querySelector('input[type=search]');input.value='room 25';input.dispatchEvent(new dom.window.Event('input'));
 assert.equal(visible(dom).length,1);assert.match(dom.window.document.querySelector('[role=status]').textContent,/1–1 of 1/);
 dom.window.document.querySelector('.hub-table-tools button').click();assert.equal(visible(dom).length,10);assert.equal(input.value,'');
 }finally{dom.window.close();}
});
test('zero matches show an empty state and disable navigation',async()=>{
 const dom=await setup();try{const input=dom.window.document.querySelector('input');input.value='missing';input.dispatchEvent(new dom.window.Event('input'));
 assert.equal(visible(dom).length,0);assert.equal(dom.window.document.querySelector('.hub-empty').parentElement.hidden,false);
 assert.ok(Array.from(dom.window.document.querySelectorAll('.hub-table-pages button')).every(b=>b.disabled));
 }finally{dom.window.close();}
});
test('numeric sorting handles Room 2 before Room 10 and supports descending',async()=>{
 const dom=await setup();try{const button=dom.window.document.querySelector('.hub-sort');button.click();assert.match(visible(dom)[1].textContent,/Room 2/);button.click();assert.match(visible(dom)[0].textContent,/Room 25/);assert.equal(button.parentElement.getAttribute('aria-sort'),'descending');
 assert.equal(dom.window.document.querySelectorAll('.hub-sort').length,2);
 }finally{dom.window.close();}
});
test('textarea limit and character counter are accessible',async()=>{
 const dom=await setup();try{const input=dom.window.document.querySelector('textarea');input.value='Hello';input.dispatchEvent(new dom.window.Event('input'));assert.equal(input.maxLength,500);assert.equal(dom.window.document.querySelector('.hub-char-count').textContent,'5 / 500 characters');assert.ok(input.getAttribute('aria-describedby'));
 }finally{dom.window.close();}
});
test('duplicate submits are stopped without disabling named submit values',async()=>{
 const dom=await setup();try{const form=dom.window.document.querySelector('form');const event=()=>new dom.window.Event('submit',{bubbles:true,cancelable:true});
 assert.equal(form.dispatchEvent(event()),true);assert.equal(form.getAttribute('aria-busy'),'true');assert.equal(form.dispatchEvent(event()),false);assert.equal(form.querySelector('button').disabled,false);
 dom.window.dispatchEvent(new dom.window.Event('pageshow'));assert.equal(form.hasAttribute('aria-busy'),false);
 }finally{dom.window.close();}
});
test('invalid and cancelled submissions do not lock forms',async()=>{
 const dom=await setup();try{const form=dom.window.document.querySelector('form');form.querySelector('textarea').required=true;
 form.dispatchEvent(new dom.window.Event('submit',{bubbles:true,cancelable:true}));assert.equal(form.hasAttribute('aria-busy'),false);
 form.querySelector('textarea').value='valid';const e=new dom.window.Event('submit',{bubbles:true,cancelable:true});e.preventDefault();form.dispatchEvent(e);assert.equal(form.hasAttribute('aria-busy'),false);
 }finally{dom.window.close();}
});
test('print temporarily reveals all filtered rows',async()=>{
 const dom=await setup();try{dom.window.dispatchEvent(new dom.window.Event('beforeprint'));assert.equal(visible(dom).length,25);dom.window.dispatchEvent(new dom.window.Event('afterprint'));assert.equal(visible(dom).length,10);
 }finally{dom.window.close();}
});
test('feedback messages expose alert/status semantics',async()=>{
 const dom=await setup();try{assert.equal(dom.window.document.querySelector('.alert-danger').getAttribute('role'),'alert');assert.equal(dom.window.document.querySelector('.alert-success').getAttribute('role'),'status');}finally{dom.window.close();}
});
