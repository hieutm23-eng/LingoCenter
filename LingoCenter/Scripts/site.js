document.addEventListener('DOMContentLoaded',function(){
 var menu=document.querySelector('.mobile-menu');if(menu)menu.addEventListener('click',function(){document.querySelector('.sidebar').classList.toggle('open')});
 document.querySelectorAll('form[data-confirm]').forEach(function(f){f.addEventListener('submit',function(e){if(!confirm(f.dataset.confirm))e.preventDefault()})});
 document.querySelectorAll('[data-print]').forEach(function(b){b.addEventListener('click',function(){window.print()})});
 var current=location.pathname.split('/')[1].toLowerCase();document.querySelectorAll('.sidebar nav a').forEach(function(a){if(a.pathname.split('/')[1].toLowerCase()===current)a.classList.add('active')});
});
